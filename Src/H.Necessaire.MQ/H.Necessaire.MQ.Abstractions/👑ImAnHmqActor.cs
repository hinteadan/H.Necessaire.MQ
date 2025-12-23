using System.Threading.Tasks;

namespace H.Necessaire.MQ.Abstractions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "H's semantic naming")]
    public interface ImAnHmqActor : ImAnHmqActorIdentity
    {
        Task<OperationResult> Raise(HmqEvent hmqEvent);
    }
}
