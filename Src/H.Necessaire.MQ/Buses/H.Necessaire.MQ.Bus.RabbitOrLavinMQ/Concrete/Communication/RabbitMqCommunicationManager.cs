using H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete.Communication.Configuration;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete.Communication
{
    internal class RabbitMqCommunicationManager : ImADependency
    {
        ImALogger logger;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            logger = dependencyProvider.GetLogger<RabbitMqCommunicationManager>();
        }

        public Task<OperationResult<IChannel>> GetChannel(RabbitMqConnectionConfiguration rabbitMqConfiguration)
        {
            return OperationResult.Fail("Noy yet implemented").WithoutPayload<IChannel>().AsTask();
        }
    }
}
