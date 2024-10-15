namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete.Communication
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry

                .Register<RabbitMqCommunicationManager>(() => new RabbitMqCommunicationManager())

                ;
        }
    }
}
