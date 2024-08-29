using H.Necessaire.MQ.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace H.Necessaire.MQ.Concrete.Storage
{
    internal class InMemHmqEventReActionStorageService : InMemoryStorageServiceBase<Guid, HmqEventReactionLog, HmqEventReActionFilter>
    {
        protected override IEnumerable<HmqEventReactionLog> ApplyFilter(IEnumerable<HmqEventReactionLog> stream, HmqEventReActionFilter filter)
        {
            if (filter?.IDs?.Any() == true)
                stream = stream.Where(x => x.ID.In(filter.IDs));

            if (filter?.From != null)
                stream = stream.Where(x => x.AsOf >= filter.From);

            if (filter?.To != null)
                stream = stream.Where(x => x.AsOf <= filter.To);

            if (filter?.EventIDs?.Any() == true)
                stream = stream.Where(x => x.EventID.In(filter.EventIDs));

            if (filter?.EventsThatHappenedFrom != null)
                stream = stream.Where(x => x.EventHappenedAt >= filter.EventsThatHappenedFrom);

            if (filter?.EventsThatHappenedTo != null)
                stream = stream.Where(x => x.EventHappenedAt <= filter.EventsThatHappenedTo);

            if (filter?.ActorIDs?.Any() == true)
                stream = stream.Where(x => x.ActorID.In(filter.ActorIDs, (item, key) => item.Is(key)));

            if (filter?.IsSuccessful != null)
                stream = stream.Where(x => x.IsSuccessful == filter.IsSuccessful.Value);

            return stream;
        }
    }
}
