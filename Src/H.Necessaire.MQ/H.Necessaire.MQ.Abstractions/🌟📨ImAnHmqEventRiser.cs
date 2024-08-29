using System.Threading.Tasks;

namespace H.Necessaire.MQ.Abstractions
{
    public interface ImAnHmqEventRiser
    {
        Task<OperationResult<ImAnHmqReActor>[]> Raise(HmqEvent hmqEvent);
    }
}
