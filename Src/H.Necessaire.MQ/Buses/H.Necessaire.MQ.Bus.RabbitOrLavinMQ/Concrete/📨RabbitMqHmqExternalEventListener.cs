using H.Necessaire.MQ.Abstractions;
using H.Necessaire.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete
{
    [ID("RabbitMQ")]
    [Alias("rabbit-mq", "rabbit")]
    internal class RabbitMqHmqExternalEventListener : ImAnHmqExternalEventListener, ImADependency, IDisposable
    {
        string routingKey = "hmq";
        string exchange = "hmq";
        ConnectionFactory rabbitMqConnectionFactory;
        IConnection rabbitMqConnection;
        IChannel rabbitMqChannel;
        AsyncEventingBasicConsumer eventConsumer;
        ImAnHmqEventRiser internalEventRiser;
        ImALogger logger;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            ConfigNode config
                = dependencyProvider
                .GetRuntimeConfig()
                ?.Get("HMQ")
                ?.Get("RabbitMQ")
                ;

            rabbitMqConnectionFactory = new ConnectionFactory
            {
                HostName = config?.Get("HostName")?.ToString(),
                VirtualHost = config?.Get("VirtualHost")?.ToString(),
                UserName = config?.Get("UserName")?.ToString(),
                Password = config?.Get("Password")?.ToString(),
            };

            string routingKeyFromConfig = config?.Get("RoutingKey")?.ToString();
            routingKey = !routingKeyFromConfig.IsEmpty() ? routingKeyFromConfig : routingKey;

            string exchangeFromConfig = config?.Get("Exchange")?.ToString();
            exchange = !exchangeFromConfig.IsEmpty() ? exchangeFromConfig : exchange;

            internalEventRiser = dependencyProvider.Build<ImAnHmqEventRiser>("internal");
            logger = dependencyProvider.GetLogger<RabbitMqHmqExternalEventListener>();
        }

        public async Task<OperationResult> Start()
        {
            rabbitMqConnection = await rabbitMqConnectionFactory.CreateConnectionAsync();
            rabbitMqChannel = await rabbitMqConnection.CreateChannelAsync();

            await rabbitMqChannel.ExchangeDeclareAsync(exchange, ExchangeType.Direct);
            QueueDeclareOk queue = await rabbitMqChannel.QueueDeclareAsync();

            await rabbitMqChannel
                .QueueBindAsync(
                    queue: queue.QueueName,
                    exchange: exchange,
                    routingKey: routingKey
                );

            eventConsumer = new AsyncEventingBasicConsumer(rabbitMqChannel);

            eventConsumer.ReceivedAsync += EventConsumer_Received;

            await rabbitMqChannel
                .BasicConsumeAsync(
                    queue: queue.QueueName,
                    autoAck: true,
                    consumer: eventConsumer
                );

            return OperationResult.Win();
        }

        public Task<OperationResult> Stop()
        {
            eventConsumer.ReceivedAsync -= EventConsumer_Received;
            eventConsumer = null;
            rabbitMqChannel.Dispose();
            rabbitMqConnection.Dispose();
            return OperationResult.Win().AsTask();
        }

        public void Dispose()
        {
            new Action(() =>
            {
                Stop().ConfigureAwait(false).GetAwaiter().GetResult();

            }).TryOrFailWithGrace();
        }


        private async Task EventConsumer_Received(object sender, BasicDeliverEventArgs args)
        {
            await
                new Func<Task>(async () =>
                {

                    byte[] body = args.Body.ToArray();
                    string hmqEventAsJsonString = Encoding.UTF8.GetString(body);
                    HmqEvent hmqEvent = hmqEventAsJsonString.TryJsonToObject<HmqEvent>().ThrowOnFailOrReturn();
                    await internalEventRiser.Raise(hmqEvent);

                })
                .TryOrFailWithGrace(onFail: async ex =>
                {
                    string body = null;
                    new Action(() => body = Encoding.UTF8.GetString(args.Body.ToArray())).TryOrFailWithGrace();
                    await logger.LogError($"Error occurred while trying to process received event from RabbitMQ; probably the payload is not an HmqEvent and therefor cannot be parsed.{Environment.NewLine}Body: {body ?? "~~N/A~~"}{Environment.NewLine}Message: {ex.Message}", ex, args.ToJsonObject(isPrettyPrinted: true) as object);
                });
        }
    }
}
