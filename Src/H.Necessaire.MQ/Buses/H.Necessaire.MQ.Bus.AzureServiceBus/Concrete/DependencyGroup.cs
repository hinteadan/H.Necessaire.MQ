using H.Necessaire.MQ.Abstractions;

namespace H.Necessaire.MQ.Bus.AzureServiceBus.Concrete
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry

                .Register<AzureServiceBusHmqEventRiser>(() => new AzureServiceBusHmqEventRiser())
                .Register<ImAnHmqEventRiser>(() => dependencyRegistry.Get<AzureServiceBusHmqEventRiser>())

                .Register<AzureServiceBusHmqExternalEventListener>(() => new AzureServiceBusHmqExternalEventListener())

                ;
        }
    }
}
