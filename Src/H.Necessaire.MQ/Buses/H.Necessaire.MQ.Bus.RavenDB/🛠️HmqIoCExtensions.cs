using H.Necessaire.MQ.Core;

namespace H.Necessaire.MQ.Bus.RavenDB
{
    public static class HmqIoCExtensions
    {
        public static T WithHmqRavenDbMessageBus<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<DependencyGroup>(() => new DependencyGroup());
            return dependencyRegistry;
        }

        public static T StartHmqRavenDbExternalListener<T>(this T dependencyProvider) where T : ImADependencyProvider
            => dependencyProvider.StartHmqExternalListener("RavenDB");
    }
}
