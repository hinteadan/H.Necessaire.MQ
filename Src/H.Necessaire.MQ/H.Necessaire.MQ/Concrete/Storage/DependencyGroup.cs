using H.Necessaire.MQ.Abstractions;
using System;

namespace H.Necessaire.MQ.Concrete.Storage
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry

                .Register<InMemHmqEventStorageService>(() => new InMemHmqEventStorageService())
                .Register<ImAStorageService<Guid, HmqEvent>>(() => dependencyRegistry.Get<InMemHmqEventStorageService>())
                .Register<ImAStorageBrowserService<HmqEvent, HmqEventFilter>>(() => dependencyRegistry.Get<InMemHmqEventStorageService>())

                .Register<InMemHmqEventReActionStorageService>(() => new InMemHmqEventReActionStorageService())
                .Register<ImAStorageService<Guid, HmqEventReactionLog>>(() => dependencyRegistry.Get<InMemHmqEventReActionStorageService>())
                .Register<ImAStorageBrowserService<HmqEventReactionLog, HmqEventReActionFilter>>(() => dependencyRegistry.Get<InMemHmqEventReActionStorageService>())

                ;
        }
    }
}
