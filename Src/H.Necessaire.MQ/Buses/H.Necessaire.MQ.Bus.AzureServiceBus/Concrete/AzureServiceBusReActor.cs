using H.Necessaire.MQ.Abstractions;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.AzureServiceBus.Concrete
{
    internal class AzureServiceBusReActor : ImAnHmqReActor
    {
        public static readonly ImAnHmqReActor Instance = new AzureServiceBusReActor();

        const string id = "AzureServiceBusReActor-{12777008-43C4-4A11-95EB-596BDE87B013}";

        public Note[] IdentityAttributes { get; set; }

        public string ID => id;

        public Task<OperationResult> Handle(HmqEvent hmqEvent)
        {
            return OperationResult.Win().AsTask();
        }
    }
}
