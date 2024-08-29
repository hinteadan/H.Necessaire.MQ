using H.Necessaire.MQ.Abstractions;
using System;

namespace H.Necessaire.MQ.Runtime.FileSystem.Concrete.Storage
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry

                .Register<FileSystemHmqEventStorageService>(() => new FileSystemHmqEventStorageService())
                .Register<ImAStorageService<Guid, HmqEvent>>(() => dependencyRegistry.Get<FileSystemHmqEventStorageService>())
                .Register<ImAStorageBrowserService<HmqEvent, HmqEventFilter>>(() => dependencyRegistry.Get<FileSystemHmqEventStorageService>())

                .Register<FileSystemHmqEventReActionStorageService>(() => new FileSystemHmqEventReActionStorageService())
                .Register<ImAStorageService<Guid, HmqEventReactionLog>>(() => dependencyRegistry.Get<FileSystemHmqEventReActionStorageService>())
                .Register<ImAStorageBrowserService<HmqEventReactionLog, HmqEventReActionFilter>>(() => dependencyRegistry.Get<FileSystemHmqEventReActionStorageService>())

                ;
        }
    }
}
