using System.Threading.Tasks;

namespace H.Necessaire.MQ.Abstractions
{
    public interface ImAnHmqActor : ImAnHmqActorIdentity
    {
        Task<OperationResult> Raise(HmqEvent hmqEvent);
    }
}
