using H.Necessaire.MQ.Abstractions;
using H.Necessaire.Runtime.RavenDB;
using Raven.Client.Documents.Indexes;
using System;
using System.Linq;

namespace H.Necessaire.MQ.Runtime.RavenDb.Concrete.Storage
{
    internal class RavenDbHmqEventReActionStorageService : RavenDbStorageServiceBase<Guid, HmqEventReactionLog, HmqEventReActionFilter, HmqEventReactionLogFilterIndex>
    {
        protected override TDocQuery ApplyFilterGeneric<TDocQuery>(TDocQuery result, HmqEventReActionFilter filter)
        {
            if (filter?.IDs?.Any() ?? false)
            {
                result = result.WhereIn(nameof(HmqEventReactionLog.ID), filter.IDs.ToStringArray());
            }

            if (filter?.From != null)
            {
                result = result.WhereGreaterThanOrEqual(nameof(HmqEventReactionLog.AsOf), filter.From.Value);
            }

            if (filter?.To != null)
            {
                result = result.WhereLessThanOrEqual(nameof(HmqEventReactionLog.AsOf), filter.To.Value);
            }

            if (filter?.EventIDs?.Any() ?? false)
            {
                result = result.WhereIn(nameof(HmqEventReactionLog.EventID), filter.EventIDs.ToStringArray());
            }

            if (filter?.EventsThatHappenedFrom != null)
            {
                result = result.WhereGreaterThanOrEqual(nameof(HmqEventReactionLog.EventHappenedAt), filter.EventsThatHappenedFrom.Value);
            }

            if (filter?.EventsThatHappenedTo != null)
            {
                result = result.WhereLessThanOrEqual(nameof(HmqEventReactionLog.EventHappenedAt), filter.EventsThatHappenedTo.Value);
            }

            if (filter?.ActorIDs?.Any() == true)
            {
                result = result.WhereIn(nameof(HmqEventReactionLog.ActorID), filter.ActorIDs);
            }

            if (filter?.IsSuccessful != null)
            {
                result = result.WhereEquals(nameof(HmqEventReactionLog.IsSuccessful), filter.IsSuccessful.Value);
            }

            return result;
        }
    }

    internal class HmqEventReactionLogFilterIndex : AbstractIndexCreationTask<HmqEventReactionLog>
    {
        public HmqEventReactionLogFilterIndex()
        {
            Map = docs => docs.Select(doc =>
                new
                {
                    doc.ID,
                    doc.AsOf,
                    doc.EventID,
                    doc.EventHappenedAt,
                    doc.ActorID,
                    doc.IsSuccessful,
                }
            );
        }
    }
}
