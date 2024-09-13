namespace H.Necessaire.MQ.Bus.AzureServiceBus.Concrete.QdActions
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .Register<ImAnActionQer>(() => new AzureServiceBusActionQer())
                .Register<AzureServiceBusQdActionProcessingDaemon>(() => new AzureServiceBusQdActionProcessingDaemon())
                .Register<ImAQdActionQueueOnDemandRunner>(() => dependencyRegistry.Get<AzureServiceBusQdActionProcessingDaemon>())
                ;
        }
    }
}
