using H.Necessaire.MQ.Abstractions;
using H.Necessaire.Runtime.Azure.CosmosDB.Core.Resources.Abstract;
using System;
using System.Collections.Generic;

namespace H.Necessaire.MQ.Runtime.Azure.CosmosDb.Concrete.Storage
{
    internal class AzureCosmosDbHmqEventStorageService
        : AzureCosmosDbStorageResourceBase<Guid, HmqEvent, HmqEventFilter>
    {
        protected override IDictionary<string, string> FilterToStoreMapping
            => new Dictionary<string, string>() {
                { nameof(HmqEventFilter.From), nameof(HmqEvent.HappenedAt) },
                { nameof(HmqEventFilter.To), nameof(HmqEvent.HappenedAt) },
                { nameof(HmqEventFilter.Assemblies), nameof(HmqEvent.Assembly) },
            };
    }
}
