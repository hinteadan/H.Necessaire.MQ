using H.Necessaire.MQ.Bus.QdActions.Commons;
using H.Necessaire.Serialization;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete.QdActions
{
    internal class RabbitMqActionQer : MessageBrokerActionQerBase, ImADependency
    {
        string queueName = "h-qd-action-queue";
        string routingKey = "h-qd-action-queue";
        ConnectionFactory rabbitMqConnectionFactory;
        ImALogger logger;
        readonly ConcurrentQueue<QdAction> qdActionsToPublish = new ConcurrentQueue<QdAction>();
        Debouncer qdActionsToPublishDebouncer;

        public override void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            base.ReferDependencies(dependencyProvider);

            qdActionsToPublishDebouncer = qdActionsToPublishDebouncer ?? new Debouncer(PublishQdActionsToMessageBroker, TimeSpan.FromSeconds(.5));

            ConfigNode config
                = dependencyProvider
                .GetRuntimeConfig()
                ?.Get("QdActions")
                ?.Get("RabbitMQ")
                ;

            string url = config?.Get("URL")?.ToString();

            if (!url.IsEmpty())
                rabbitMqConnectionFactory = new ConnectionFactory { Uri = new Uri(url), };

            rabbitMqConnectionFactory = rabbitMqConnectionFactory ?? new ConnectionFactory
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

            logger = dependencyProvider.GetLogger<RabbitMqActionQer>();
        }

        protected override async Task<OperationResult> QueueActionToMessageBroker(QdAction action)
        {
            OperationResult result = OperationResult.Fail("Not yet started");

            qdActionsToPublish.Enqueue(action);

            await
                new Func<Task>(async () =>
                {

                    await qdActionsToPublishDebouncer.Invoke();

                    result = OperationResult.Win();

                })
                .TryOrFailWithGrace(onFail: async ex =>
                {
                    await logger.LogError(ex);
                    result = OperationResult.Fail(ex, $"Error occurred while queueing QD Action {action}. Message: {ex.Message}");
                });

            return result;
        }

        private async Task PublishQdActionsToMessageBroker()
        {
            if (qdActionsToPublish.Count == 0)
                return;

            await new Func<Task>(async () =>
            {
                using (IConnection rabbitMqConnection = await rabbitMqConnectionFactory.CreateConnectionAsync())
                {
                    using (IChannel rabbitMqChannel = await rabbitMqConnection.CreateChannelAsync())
                    {
                        QueueDeclareOk queue = await rabbitMqChannel.QueueDeclareAsync(
                            queue: queueName,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null
                        );

                        var messageProperties = new BasicProperties { Persistent = true };

                        while(qdActionsToPublish.TryDequeue(out QdAction action))
                        {
                            await rabbitMqChannel.BasicPublishAsync(
                                exchange: string.Empty,
                                routingKey: routingKey,
                                basicProperties: messageProperties,
                                mandatory: true,
                                body: Encoding.UTF8.GetBytes(action.ToJsonObject())
                            );
                        }
                    }
                }
            })
            .TryOrFailWithGrace(onFail: async ex =>
            {
                await logger.LogError($"Error occurred while publishing QD Actions queue. Reason: {ex.Message}", ex);
            });
        }
    }
}
