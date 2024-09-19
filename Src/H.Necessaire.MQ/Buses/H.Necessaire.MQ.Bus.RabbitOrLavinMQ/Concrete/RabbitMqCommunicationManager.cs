using RabbitMQ.Client;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete
{
    internal class RabbitMqCommunicationManager : ImADependency
    {
        ImALogger logger;
        RabbitMqConfiguration rabbitMqConfiguration;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            logger = dependencyProvider.GetLogger<RabbitMqCommunicationManager>();
        }

        public Task<OperationResult<IModel>> GetChannel(RabbitMqConfiguration rabbitMqConfiguration)
        {
            return OperationResult.Fail("Noy yet implemented").WithoutPayload<IModel>().AsTask();
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
