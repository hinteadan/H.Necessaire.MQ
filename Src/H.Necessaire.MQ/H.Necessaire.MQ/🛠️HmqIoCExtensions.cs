using H.Necessaire.MQ.Core;

namespace H.Necessaire.MQ
{
    public static class HmqIoCExtensions
    {
        public static T WithHmq<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<DependencyGroup>(() => new DependencyGroup());
            return dependencyRegistry;
        }

        public static T StartHmqPeriodicPollingExternalListener<T>(this T dependencyProvider) where T : ImADependencyProvider
            => dependencyProvider.StartHmqExternalListener("PeriodicPolling");
    }
}
