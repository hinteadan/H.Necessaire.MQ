using H.Necessaire.MQ.Bus.AzureServiceBus.Concrete.QdActions;

namespace H.Necessaire.MQ.Bus.AzureServiceBus
{
    public static class QdActionIoCExtensions
    {
        public static T WithAzureServiceBusQdActions<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<Concrete.QdActions.DependencyGroup>(() => new Concrete.QdActions.DependencyGroup());
            return dependencyRegistry;
        }

        public static T StartAzureServiceBusQdActionsProcessor<T>(this T dependencyProvider) where T : ImADependencyProvider
        {
            ImADaemon azureServiceBusQdActionsProcessingDaemon = dependencyProvider.Get<AzureServiceBusQdActionProcessingDaemon>();

            azureServiceBusQdActionsProcessingDaemon
                .Start()
                .ConfigureAwait(continueOnCapturedContext: false)
                .GetAwaiter()
                .GetResult()
                ;

            return dependencyProvider;
        }

        public static T StopAzureServiceBusQdActionsProcessor<T>(this T dependencyProvider) where T : ImADependencyProvider
        {
            ImADaemon azureServiceBusQdActionsProcessingDaemon = dependencyProvider.Get<AzureServiceBusQdActionProcessingDaemon>();

            azureServiceBusQdActionsProcessingDaemon
                .Stop()
                .ConfigureAwait(continueOnCapturedContext: false)
                .GetAwaiter()
                .GetResult()
                ;

            return dependencyProvider;
        }
    }
}
