using H.Necessaire.MQ.Abstractions;
using H.Necessaire.Runtime.Azure.CosmosDB.Core.Resources.Abstract;
using System;
using System.Collections.Generic;

namespace H.Necessaire.MQ.Runtime.Azure.CosmosDb.Concrete.Storage
{
    internal class AzureCosmosDbHmqEventReActionStorageService
        : AzureCosmosDbStorageResourceBase<Guid, HmqEventReactionLog, HmqEventReActionFilter>
    {
        protected override IDictionary<string, string> FilterToStoreMapping
            => new Dictionary<string, string>() {
                { nameof(HmqEventReActionFilter.From), nameof(HmqEventReactionLog.AsOf) },
                { nameof(HmqEventReActionFilter.To), nameof(HmqEventReactionLog.AsOf) },
                { nameof(HmqEventReActionFilter.EventsThatHappenedFrom), nameof(HmqEventReactionLog.EventHappenedAt) },
                { nameof(HmqEventReActionFilter.EventsThatHappenedTo), nameof(HmqEventReactionLog.EventHappenedAt) },
            };
    }
}
