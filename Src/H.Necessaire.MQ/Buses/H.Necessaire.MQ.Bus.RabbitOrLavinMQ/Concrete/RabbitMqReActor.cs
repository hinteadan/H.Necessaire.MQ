using H.Necessaire.MQ.Abstractions;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete
{
    internal class RabbitMqReActor : ImAnHmqReActor
    {
        public static readonly ImAnHmqReActor Instance = new RabbitMqReActor();

        const string id = "RabbitMqReActor-{AF0B1B76-269D-4527-8526-22F1F061441A}";

        public Note[] IdentityAttributes { get; set; }

        public string ID => id;

        public Task<OperationResult> Handle(HmqEvent hmqEvent)
        {
            return OperationResult.Win().AsTask();
        }
    }
}
