using H.Necessaire.MQ.Abstractions;

namespace H.Necessaire.MQ.Bus.RavenDB.Concrete
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .Register<Storage.DependencyGroup>(() => new Storage.DependencyGroup())

                .Register<RavenDbHmqEventRiser>(() => new RavenDbHmqEventRiser())
                .Register<ImAnHmqEventRiser>(() => dependencyRegistry.Get<RavenDbHmqEventRiser>())

                .Register<RavenDbHmqExternalEventListener>(() => new RavenDbHmqExternalEventListener())

                ;
        }
    }
}
