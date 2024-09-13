using H.Necessaire.Serialization;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete.QdActions
{
    internal class RabbitMqActionQer : ImAnActionQer, ImADependency
    {
        string queueName = "h-qd-action-queue";
        string routingKey = "h-qd-action-queue";
        ConnectionFactory rabbitMqConnectionFactory;
        IConnection rabbitMqConnection;
        IModel rabbitMqChannel;
        ImALogger logger;

        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            ConfigNode config
                = dependencyProvider
                .GetRuntimeConfig()
                ?.Get("QdActions")
                ?.Get("RabbitMQ")
                ;

            rabbitMqConnectionFactory = new ConnectionFactory
            {
                HostName = config?.Get("HostName")?.ToString(),
                VirtualHost = config?.Get("VirtualHost")?.ToString(),
                UserName = config?.Get("UserName")?.ToString(),
                Password = config?.Get("Password")?.ToString(),
            };

            string queueNameFromConfig = config?.Get("QueueName")?.ToString();
            queueName = !queueNameFromConfig.IsEmpty() ? queueNameFromConfig : queueName;

            string routingKeyFromConfig = config?.Get("RoutingKey")?.ToString();
            routingKey = !routingKeyFromConfig.IsEmpty() ? routingKeyFromConfig : routingKey;

            logger = dependencyProvider.GetLogger<RabbitMqActionQer>();
        }

        public async Task<OperationResult> Queue(QdAction action)
        {
            OperationResult result = OperationResult.Fail("Not yet started");

            await
                new Func<Task>(() =>
                {

                    using (IConnection rabbitMqConenction = rabbitMqConnectionFactory.CreateConnection())
                    {
                        using (IModel rabbitMqChannel = rabbitMqConenction.CreateModel())
                        {
                            QueueDeclareOk queue = rabbitMqChannel.QueueDeclare(
                                queue: queueName,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null
                            );

                            rabbitMqChannel.BasicPublish(
                                exchange: string.Empty,
                                routingKey: routingKey,
                                basicProperties: rabbitMqChannel.CreateBasicProperties().And(x => x.Persistent = true),
                                body: Encoding.UTF8.GetBytes(action.ToJsonObject())
                            );
                        }
                    }

                    result = OperationResult.Win();

                    return true.AsTask();

                })
                .TryOrFailWithGrace(onFail: async ex =>
                {
                    await logger.LogError(ex);
                    result = OperationResult.Fail(ex, $"Error occurred while queueing QD Action {action}. Message: {ex.Message}");
                });

            return result;
        }
    }
}
