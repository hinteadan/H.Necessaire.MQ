using H.Necessaire.MQ.Abstractions;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.RavenDB.Concrete
{
    internal class RavenDbReActor : ImAnHmqReActor
    {
        public static readonly ImAnHmqReActor Instance = new RavenDbReActor();

        const string id = "RavenDbReActor-{10C4BE32-2E4B-4165-AD35-3510A1B37501}";

        public Note[] IdentityAttributes { get; set; }

        public string ID => id;

        public Task<OperationResult> Handle(HmqEvent hmqEvent)
        {
            return OperationResult.Win().AsTask();
        }
    }
}
