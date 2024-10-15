using H.Necessaire.MQ.Abstractions;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry

                .Register<Communication.DependencyGroup>(() => new Communication.DependencyGroup())

                .Register<Commons.DependencyGroup>(() => new Commons.DependencyGroup())

                .Register<RabbitMqHmqEventRiser>(() => new RabbitMqHmqEventRiser())
                .Register<ImAnHmqEventRiser>(() => dependencyRegistry.Get<RabbitMqHmqEventRiser>())

                .Register<RabbitMqHmqExternalEventListener>(() => new RabbitMqHmqExternalEventListener())

                ;
        }
    }
}
