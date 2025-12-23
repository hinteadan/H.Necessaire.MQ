using H.Necessaire.MQ.Agent.Abstractions;
using System;

namespace H.Necessaire.MQ.Agent.Core.Concrete
{
    internal class HmqAgentInfo : EphemeralTypeBase, ImAnHmqAgentInfo
    {
        public HmqAgentInfo() => DoNotExpire();

        public string ID { get; set; } = Guid.NewGuid().ToString();
        public Note[] IdentityAttributes { get; set; }
    }
}
