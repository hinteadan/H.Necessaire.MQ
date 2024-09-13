namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete.QdActions
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .Register<ImAnActionQer>(() => new RabbitMqActionQer())
                .Register<RabbitMqQdActionProcessingDaemon>(() => new RabbitMqQdActionProcessingDaemon())
                .Register<ImAQdActionQueueOnDemandRunner>(() => dependencyRegistry.Get<RabbitMqQdActionProcessingDaemon>())
                ;
        }
    }
}
