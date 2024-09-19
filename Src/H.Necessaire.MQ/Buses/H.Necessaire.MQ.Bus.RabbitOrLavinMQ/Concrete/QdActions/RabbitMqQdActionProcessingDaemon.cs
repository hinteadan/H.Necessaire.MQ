using H.Necessaire.MQ.Bus.Commons;
using H.Necessaire.MQ.Bus.QdActions.Commons;
using H.Necessaire.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete.QdActions
{
    [ID("RabbitMq")]
    [Alias("rabbit-mq", "rabbit", "lavin-mq", "lavin")]
    internal class RabbitMqQdActionProcessingDaemon : MessageBrokerNotifiedQdActionProcessingDaemonBase
    {
        string queueName = "h-qd-action-queue";
        string routingKey = "h-qd-action-queue";
        ConnectionFactory rabbitMqConnectionFactory;
        IConnection rabbitMqConnection;
        IModel rabbitMqChannel;
        EventingBasicConsumer eventConsumer;
        ImALogger logger;
        ImAResilienceRecoveryRegistry resilienceRecoveryRegistry;

        public override void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            base.ReferDependencies(dependencyProvider);

            logger = dependencyProvider.GetLogger<RabbitMqQdActionProcessingDaemon>();

            ConfigNode config
                = dependencyProvider
                .GetRuntimeConfig()
                ?.Get("QdActions")
                ?.Get("RabbitMQ")
                ;

            rabbitMqConnectionFactory = new ConnectionFactory
            {
                HostName = config?.Get("HostName")?.ToString() ?? "",
                VirtualHost = config?.Get("VirtualHost")?.ToString(),
                UserName = config?.Get("UserName")?.ToString() ?? "",
                Password = config?.Get("Password")?.ToString() ?? "",
            };

            string queueNameFromConfig = config?.Get("QueueName")?.ToString();
            queueName = !queueNameFromConfig.IsEmpty() ? queueNameFromConfig : queueName;

            string routingKeyFromConfig = config?.Get("RoutingKey")?.ToString();
            routingKey = !routingKeyFromConfig.IsEmpty() ? routingKeyFromConfig : routingKey;

            uint? prefetchCountFromConfig = config?.Get("PrefetchCount")?.ToString()?.ParseToUIntOrFallbackTo(null);
            maxConcurrentMessageHandling = (prefetchCountFromConfig == null) ? maxConcurrentMessageHandling : (ushort)prefetchCountFromConfig.Value;

            resilienceRecoveryRegistry = dependencyProvider.Get<ImAResilienceRecoveryRegistry>();
        }

        public override Task Start(CancellationToken? cancellationToken = null)
        {
            int retryAttempt = 0;
            new Action(() =>
            {
                rabbitMqConnection = rabbitMqConnectionFactory.CreateConnection();
            })
            .TryOrFailWithGrace(
                numberOfTimes: 2,
                onFail: async ex => await logger.LogError($"Error occurred while trying to connect to RabbitMQ after {retryAttempt + 1} attempt(s). Reason: {ex.Message}", ex),
                onRetry: async ex => await logger.LogWarn($"Couldn't connect to RabbitMQ, retrying (attempt {++retryAttempt})... Reason: {ex.Message}", ex),
                millisecondsToSleepBetweenRetries: 1000
            );

            resilienceRecoveryRegistry.RegisterResilienceTask(RunResiliencyChecks);

            if (rabbitMqConnection is null)
                return false.AsTask();

            rabbitMqConnection.ConnectionShutdown += RabbitMqConnection_ConnectionShutdown;
            rabbitMqConnection.ConnectionBlocked += RabbitMqConnection_ConnectionBlocked;
            rabbitMqConnection.ConnectionUnblocked += RabbitMqConnection_ConnectionUnblocked;
            rabbitMqConnection.CallbackException += RabbitMqConnection_CallbackException;

            rabbitMqChannel = rabbitMqConnection.CreateModel();
            rabbitMqChannel.ModelShutdown += RabbitMqChannel_ModelShutdown;

            QueueDeclareOk queue = rabbitMqChannel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: maxConcurrentMessageHandling, global: false);

            eventConsumer = new EventingBasicConsumer(rabbitMqChannel);

            eventConsumer.Received += EventConsumer_Received;

            rabbitMqChannel
                .BasicConsume(
                    queue: queue.QueueName,
                    autoAck: false,
                    consumer: eventConsumer
                );

            return true.AsTask();
        }

        public override Task Stop(CancellationToken? cancellationToken = null)
        {
            resilienceRecoveryRegistry.UnregisterResilienceTask(RunResiliencyChecks);

            if (rabbitMqConnection is null)
                return false.AsTask();

            new Action(() =>
            {
                eventConsumer.Received -= EventConsumer_Received;
                eventConsumer = null;
            })
            .TryOrFailWithGrace();

            new Action(() =>
            {
                rabbitMqChannel.ModelShutdown -= RabbitMqChannel_ModelShutdown;
                rabbitMqChannel.Dispose();
                rabbitMqChannel = null;
            })
            .TryOrFailWithGrace();

            new Action(() =>
            {
                rabbitMqConnection.ConnectionShutdown -= RabbitMqConnection_ConnectionShutdown;
                rabbitMqConnection.ConnectionBlocked -= RabbitMqConnection_ConnectionBlocked;
                rabbitMqConnection.ConnectionUnblocked -= RabbitMqConnection_ConnectionUnblocked;
                rabbitMqConnection.CallbackException -= RabbitMqConnection_CallbackException;
                rabbitMqConnection.Dispose();
                rabbitMqConnection = null;
            })
            .TryOrFailWithGrace();

            return true.AsTask();
        }

        private async void RabbitMqConnection_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            await logger.LogError($"Error occurred on the RabbitMQ Connection callback. Details: {e.Exception?.Message}", e.Exception, payload: null, e.Detail?.Select(x => (x.Value?.ToString()).NoteAs(x.Key)).ToArrayNullIfEmpty());
        }

        private async void RabbitMqConnection_ConnectionUnblocked(object sender, EventArgs e)
        {
            await logger.LogWarn($"RabbitMQ Connection un-blocked");
        }

        private async void RabbitMqConnection_ConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            await logger.LogWarn($"RabbitMQ Connection blocked. Reason: {e.Reason}");
        }

        private async void RabbitMqConnection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            await logger.LogError($"Error occurred on the RabbitMQ Connection and it was shutdown. Will try to re-establish. Details: {e}", e.Exception);
            await Stop();
            await Start();
        }

        private async void RabbitMqChannel_ModelShutdown(object sender, ShutdownEventArgs e)
        {
            await logger.LogError($"Error occurred on the RabbitMQ connection's Model and it was shutdown. Will try to re-establish. Details: {e}", e.Exception);
            await Stop();
            await Start();
        }

        private async void EventConsumer_Received(object sender, BasicDeliverEventArgs args)
        {
            byte[] body = args.Body.ToArray();

            string qdActionAsJsonString = Encoding.UTF8.GetString(body);

            QdAction qdAction = qdActionAsJsonString.TryJsonToObject<QdAction>().ThrowOnFailOrReturn();

            await ProcessQdActionProcessingResult(
                await ProcessQdAction(qdAction), 
                failMarker: () => { rabbitMqChannel.BasicNack(deliveryTag: args.DeliveryTag, multiple: false, requeue: false); return true.AsTask(); },
                winMarker: () => { rabbitMqChannel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false); return true.AsTask(); }
            );
        }

        private async Task RunResiliencyChecks()
        {
            await logger.LogTrace($"Running RabbitMQ Resiliency Checks");
            TimeSpan duration = TimeSpan.Zero;
            using (new TimeMeasurement(x => duration = x))
            {
                if (rabbitMqConnection != null)
                    return;

                await Stop();
                await Start();
            }
            await logger.LogTrace($"DONE Running RabbitMQ Resiliency Checks in {duration}");
        }
    }
}
