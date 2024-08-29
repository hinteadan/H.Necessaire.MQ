using H.Necessaire.MQ.Abstractions;
using System;

namespace H.Necessaire.MQ.Runtime.Google.FirestoreDB.Concrete.Storage
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry

                .Register<GoogleFirestoreDbHmqEventStorageService>(() => new GoogleFirestoreDbHmqEventStorageService())
                .Register<ImAStorageService<Guid, HmqEvent>>(() => dependencyRegistry.Get<GoogleFirestoreDbHmqEventStorageService>())
                .Register<ImAStorageBrowserService<HmqEvent, HmqEventFilter>>(() => dependencyRegistry.Get<GoogleFirestoreDbHmqEventStorageService>())

                .Register<GoogleFirestoreDbHmqEventReActionStorageService>(() => new GoogleFirestoreDbHmqEventReActionStorageService())
                .Register<ImAStorageService<Guid, HmqEventReactionLog>>(() => dependencyRegistry.Get<GoogleFirestoreDbHmqEventReActionStorageService>())
                .Register<ImAStorageBrowserService<HmqEventReactionLog, HmqEventReActionFilter>>(() => dependencyRegistry.Get<GoogleFirestoreDbHmqEventReActionStorageService>())

                ;
        }
    }
}
