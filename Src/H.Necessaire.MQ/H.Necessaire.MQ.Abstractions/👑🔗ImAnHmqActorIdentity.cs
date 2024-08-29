namespace H.Necessaire.MQ.Abstractions
{
    public interface ImAnHmqActorIdentity : IStringIdentity
    {
        Note[] IdentityAttributes { get; }
    }
}
