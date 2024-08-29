using H.Necessaire.MQ.Abstractions;
using H.Necessaire.Runtime.RavenDB;
using Raven.Client.Documents.Indexes;
using System;
using System.Linq;

namespace H.Necessaire.MQ.Runtime.RavenDb.Concrete.Storage
{
    internal class RavenDbHmqEventStorageService : RavenDbStorageServiceBase<Guid, HmqEvent, HmqEventFilter, HmqEventFilterIndex>
    {
        protected override TDocQuery ApplyFilterGeneric<TDocQuery>(TDocQuery result, HmqEventFilter filter)
        {
            if (filter?.IDs?.Any() ?? false)
            {
                result = result.WhereIn(nameof(HmqEvent.ID), filter.IDs.ToStringArray());
            }

            if (filter?.From != null)
            {
                result = result.WhereGreaterThanOrEqual(nameof(HmqEvent.HappenedAt), filter.From.Value);
            }

            if (filter?.To != null)
            {
                result = result.WhereLessThanOrEqual(nameof(HmqEvent.HappenedAt), filter.To.Value);
            }

            if (filter?.Names?.Any() ?? false)
            {
                result = result.WhereIn(nameof(HmqEvent.Name), filter.Names);
            }

            if (filter?.Types?.Any() ?? false)
            {
                result = result.WhereIn(nameof(HmqEvent.Type), filter.Types);
            }

            if (filter?.Assemblies?.Any() ?? false)
            {
                result = result.WhereIn(nameof(HmqEvent.Assembly), filter.Assemblies);
            }

            return result;
        }
    }

    internal class HmqEventFilterIndex : AbstractIndexCreationTask<HmqEvent>
    {
        public HmqEventFilterIndex()
        {
            Map = docs => docs.Select(doc =>
                new
                {
                    doc.ID,
                    doc.HappenedAt,
                    doc.Name,
                    doc.Type,
                    doc.Assembly,
                }
            );
        }
    }
}
