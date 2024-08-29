using H.Necessaire.MQ.Core;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ
{
    public static class HmqIoCExtensions
    {
        public static T WithHmqRabbitMqMessageBus<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<DependencyGroup>(() => new DependencyGroup());
            return dependencyRegistry;
        }

        public static T StartHmqRabbitMqExternalListener<T>(this T dependencyProvider) where T : ImADependencyProvider
            => dependencyProvider.StartHmqExternalListener("RabbitMQ");
    }
}
