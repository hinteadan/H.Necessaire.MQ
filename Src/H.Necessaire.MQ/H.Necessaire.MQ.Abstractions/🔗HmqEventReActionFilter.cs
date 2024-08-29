using System;

namespace H.Necessaire.MQ.Abstractions
{
    public class HmqEventReActionFilter : SortFilterBase, IPageFilter
    {
        static readonly string[] validSortNames = new string[] {
            nameof(HmqEventReactionLog.ID),
            nameof(HmqEventReactionLog.AsOf),
            nameof(HmqEventReactionLog.EventID),
            nameof(HmqEventReactionLog.EventHappenedAt),
            nameof(HmqEventReactionLog.ActorID),
            nameof(HmqEventReactionLog.IsSuccessful),
        };
        public PageFilter PageFilter { get; set; }
        protected override string[] ValidSortNames => validSortNames;

        public Guid[] IDs { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public Guid[] EventIDs { get; set; }
        public DateTime? EventsThatHappenedFrom { get; set; }
        public DateTime? EventsThatHappenedTo { get; set; }

        public string[] ActorIDs { get; set; }

        public bool? IsSuccessful { get; set; }
    }
}
