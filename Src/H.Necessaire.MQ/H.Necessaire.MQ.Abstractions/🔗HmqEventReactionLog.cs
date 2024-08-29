using System;

namespace H.Necessaire.MQ.Abstractions
{
    public sealed class HmqEventReactionLog : IGuidIdentity
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public DateTime AsOf { get; set; } = DateTime.UtcNow;

        public HmqEvent Event { get; set; }
        public Guid EventID => Event?.ID ?? Guid.Empty;
        public DateTime EventHappenedAt => Event?.HappenedAt ?? AsOf;

        public HmqActorIdentity ActorIdentity { get; set; }
        public string ActorID => ActorIdentity?.ID;

        public OperationResult OperationResult { get; set; }
        public bool IsSuccessful => OperationResult?.IsSuccessful == true;
    }
}
