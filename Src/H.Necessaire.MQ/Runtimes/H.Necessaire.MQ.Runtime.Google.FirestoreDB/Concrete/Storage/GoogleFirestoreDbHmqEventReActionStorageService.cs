using H.Necessaire.MQ.Abstractions;
using H.Necessaire.Runtime.Google.FirestoreDB.Core.Resources.Abstract;
using System;
using System.Collections.Generic;

namespace H.Necessaire.MQ.Runtime.Google.FirestoreDB.Concrete.Storage
{
    internal class GoogleFirestoreDbHmqEventReActionStorageService
        : GoogleFirestoreDbStorageResourceBase<Guid, HmqEventReactionLog, HmqEventReActionFilter>
    {
        protected override IDictionary<string, Note> FilterToStoreMapping
            => new Dictionary<string, Note>() {
                { nameof(HmqEventReActionFilter.From), nameof(HmqEventReactionLog.AsOf).NoteAs(">=") },
                { nameof(HmqEventReActionFilter.To), nameof(HmqEventReactionLog.AsOf).NoteAs("<=") },
                { nameof(HmqEventReActionFilter.EventsThatHappenedFrom), nameof(HmqEventReactionLog.EventHappenedAt).NoteAs(">=") },
                { nameof(HmqEventReActionFilter.EventsThatHappenedTo), nameof(HmqEventReactionLog.EventHappenedAt).NoteAs("<=") },
            };
    }
}
