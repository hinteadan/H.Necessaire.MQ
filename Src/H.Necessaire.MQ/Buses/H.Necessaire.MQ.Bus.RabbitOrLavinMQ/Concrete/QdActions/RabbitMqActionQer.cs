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

        private Task PublishQdActionsToMessageBroker()
        {
            if (qdActionsToPublish.Count == 0)
                return false.AsTask();

            new Action(() =>
            {
                using (IConnection rabbitMqConnection = rabbitMqConnectionFactory.CreateConnection())
                {
                    using (IModel rabbitMqChannel = rabbitMqConnection.CreateModel())
                    {
                        QueueDeclareOk queue = rabbitMqChannel.QueueDeclare(
                            queue: queueName,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null
                        );

                        IBasicProperties messageProperties = rabbitMqChannel.CreateBasicProperties().And(x => x.Persistent = true);

                        while(qdActionsToPublish.TryDequeue(out QdAction action))
                        {
                            rabbitMqChannel.BasicPublish(
                                exchange: string.Empty,
                                routingKey: routingKey,
                                basicProperties: messageProperties,
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

            return true.AsTask();
        }
    }
}
