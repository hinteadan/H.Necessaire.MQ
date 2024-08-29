using H.Necessaire.MQ.Abstractions;
using H.Necessaire.Runtime.Google.FirestoreDB.Core.Resources.Abstract;
using System;
using System.Collections.Generic;

namespace H.Necessaire.MQ.Runtime.Google.FirestoreDB.Concrete.Storage
{
    internal class GoogleFirestoreDbHmqEventStorageService
        : GoogleFirestoreDbStorageResourceBase<Guid, HmqEvent, HmqEventFilter>
    {
        protected override IDictionary<string, Note> FilterToStoreMapping
            => new Dictionary<string, Note>() {
                { nameof(HmqEventFilter.From), nameof(HmqEvent.HappenedAt).NoteAs(">=") },
                { nameof(HmqEventFilter.To), nameof(HmqEvent.HappenedAt).NoteAs("<=") },
                { nameof(HmqEventFilter.Assemblies), nameof(HmqEvent.Assembly).NoteAs("[=]") },
            };
    }
}
