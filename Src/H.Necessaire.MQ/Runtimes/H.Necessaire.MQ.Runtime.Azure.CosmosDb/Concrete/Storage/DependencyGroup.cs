using H.Necessaire.MQ.Abstractions;
using System;

namespace H.Necessaire.MQ.Runtime.Azure.CosmosDb.Concrete.Storage
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry

                .Register<AzureCosmosDbHmqEventStorageService>(() => new AzureCosmosDbHmqEventStorageService())
                .Register<ImAStorageService<Guid, HmqEvent>>(() => dependencyRegistry.Get<AzureCosmosDbHmqEventStorageService>())
                .Register<ImAStorageBrowserService<HmqEvent, HmqEventFilter>>(() => dependencyRegistry.Get<AzureCosmosDbHmqEventStorageService>())

                .Register<AzureCosmosDbHmqEventReActionStorageService>(() => new AzureCosmosDbHmqEventReActionStorageService())
                .Register<ImAStorageService<Guid, HmqEventReactionLog>>(() => dependencyRegistry.Get<AzureCosmosDbHmqEventReActionStorageService>())
                .Register<ImAStorageBrowserService<HmqEventReactionLog, HmqEventReActionFilter>>(() => dependencyRegistry.Get<AzureCosmosDbHmqEventReActionStorageService>())

                ;
        }
    }
}
