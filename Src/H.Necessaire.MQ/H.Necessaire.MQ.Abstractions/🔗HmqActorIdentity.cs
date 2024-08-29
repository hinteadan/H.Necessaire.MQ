using System;

namespace H.Necessaire.MQ.Abstractions
{
    public class HmqActorIdentity : ImAnHmqActorIdentity
    {
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public Note[] IdentityAttributes { get; set; }
    }
}
