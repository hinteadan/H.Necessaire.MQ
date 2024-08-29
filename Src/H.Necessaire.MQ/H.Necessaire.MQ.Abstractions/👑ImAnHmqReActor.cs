using System.Threading.Tasks;

namespace H.Necessaire.MQ.Abstractions
{
    public interface ImAnHmqReActor : ImAnHmqActorIdentity
    {
        Task<OperationResult> Handle(HmqEvent hmqEvent);
    }
}
