using H.Necessaire.MQ.Abstractions;

namespace H.Necessaire.MQ.Concrete
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry

                .Register<ImAnHmqActorAndReActorBookkeeper>(() => new HmqActorAndReActorBookkeeper())
                .RegisterAlwaysNew<HmqActor>(() => new HmqActor())
                .RegisterAlwaysNew<HmqReActor>(() => new HmqReActor())

                .Register<Storage.DependencyGroup>(() => new Storage.DependencyGroup())

                .Register<HmqEventRegistry>(() => new HmqEventRegistry())
                .Register<ImAnHmqEventRegistry>(() => dependencyRegistry.Get<HmqEventRegistry>())
                .Register<ImAnHmqEventReActionRegistry>(() => dependencyRegistry.Get<HmqEventRegistry>())

                .Register<HmqEventInternalRiser>(() => new HmqEventInternalRiser())
                .Register<ImAnHmqEventRiser>(() => dependencyRegistry.Get<HmqEventInternalRiser>())

                .Register<PeriodicPollingHmqExternalEventListener>(() => new PeriodicPollingHmqExternalEventListener())

                ;
        }
    }
}
