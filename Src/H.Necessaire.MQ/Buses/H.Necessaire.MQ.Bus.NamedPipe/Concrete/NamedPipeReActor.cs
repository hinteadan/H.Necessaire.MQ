using H.Necessaire.MQ.Abstractions;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.NamedPipe.Concrete
{
    internal class NamedPipeReActor : ImAnHmqReActor
    {
        public static readonly ImAnHmqReActor Instance = new NamedPipeReActor();

        const string id = "NamedPipeReActor-{0BF35A09-0180-49E4-B56C-6035C93D383D}";

        public Note[] IdentityAttributes { get; set; }

        public string ID => id;

        public Task<OperationResult> Handle(HmqEvent hmqEvent)
        {
            return OperationResult.Win().AsTask();
        }
    }
}
