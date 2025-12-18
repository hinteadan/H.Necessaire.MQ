using H.Necessaire.MQ.Bus.Commons;
using H.Necessaire.MQ.Core;

namespace H.Necessaire.MQ.Bus.AzureServiceBus
{
    public static class HmqIoCExtensions
    {
        public static T WithHmqAzureServiceBusMessageBus<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<DependencyGroup>(() => new DependencyGroup());
            return dependencyRegistry;
        }

        public static T StartHmqAzureServiceBusExternalListener<T>(this T dependencyProvider) where T : ImADependencyProvider
        {
            dependencyProvider.StartHmqExternalListener("AzureServiceBus");
            dependencyProvider.Get<ResilienceRecoveryDaemon>().Start();
            return dependencyProvider;
        }
    }
}
