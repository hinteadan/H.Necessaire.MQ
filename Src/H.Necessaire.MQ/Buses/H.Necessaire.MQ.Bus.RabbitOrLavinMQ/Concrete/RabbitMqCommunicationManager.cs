using H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete.QdActions;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete
{
    internal class RabbitMqCommunicationManager : ImADependency
    {
        ImALogger logger;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            logger = dependencyProvider.GetLogger<RabbitMqCommunicationManager>();
        }
    }

    internal class RabbitMqConfiguration
    {
        public string HostName { get; set; }
        public string VirtualHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public QueueConfiguration Queue { get; } = new QueueConfiguration();
        public PubSubConfiguration PubSub { get; } = new PubSubConfiguration();

        public class QueueConfiguration
        {
            public string QueueName { get; set; }
            public string RoutingKey { get; set; }
            public ushort PrefetchCount { get; set; }
        }

        public class PubSubConfiguration
        {
            public string Exchange { get; set; }
            public string RoutingKey { get; set; }
        }
    }
}
