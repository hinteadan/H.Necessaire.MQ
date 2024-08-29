using System.Threading.Tasks;

namespace H.Necessaire.MQ.Abstractions
{
    public interface ImAnHmqExternalEventListener
    {
        Task<OperationResult> Start();

        Task<OperationResult> Stop();
    }
}
