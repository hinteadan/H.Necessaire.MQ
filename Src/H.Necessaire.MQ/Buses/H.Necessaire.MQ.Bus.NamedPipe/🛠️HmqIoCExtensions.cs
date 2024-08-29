using H.Necessaire.MQ.Core;

namespace H.Necessaire.MQ.Bus.NamedPipe
{
    public static class HmqIoCExtensions
    {
        public static T WithHmqNamedPipeMessageBus<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<DependencyGroup>(() => new DependencyGroup());
            return dependencyRegistry;
        }

        public static T StartHmqNamedPipeExternalListener<T>(this T dependencyProvider) where T : ImADependencyProvider
            => dependencyProvider.StartHmqExternalListener("NamedPipe");
    }
}
