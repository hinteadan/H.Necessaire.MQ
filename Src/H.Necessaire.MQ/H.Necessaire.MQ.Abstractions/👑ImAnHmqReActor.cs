using System.Threading.Tasks;

namespace H.Necessaire.MQ.Abstractions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "H's semantic naming")]
    public interface ImAnHmqReActor : ImAnHmqActorIdentity
    {
        Task<OperationResult> Handle(HmqEvent hmqEvent);
    }
}
