using H.Necessaire.MQ.Abstractions;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry

                .Register<RabbitMqHmqEventRiser>(() => new RabbitMqHmqEventRiser())
                .Register<ImAnHmqEventRiser>(() => dependencyRegistry.Get<RabbitMqHmqEventRiser>())

                .Register<RabbitMqHmqExternalEventListener>(() => new RabbitMqHmqExternalEventListener())

                ;
        }
    }
}
