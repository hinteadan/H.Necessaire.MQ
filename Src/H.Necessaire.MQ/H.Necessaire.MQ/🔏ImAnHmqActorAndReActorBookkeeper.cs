using H.Necessaire.MQ.Abstractions;
using System;

namespace H.Necessaire.MQ
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "H's semantic naming")]
    internal interface ImAnHmqActorAndReActorBookkeeper
    {
        ImAnHmqActor GetOrAddActor(string id, Func<string, ImAnHmqActor> actorBuilder);
        ImAnHmqReActor GetOrAddReActor(string id, Func<string, ImAnHmqReActor> reActorBuilder);
        ImAnHmqReActor[] GetAllReActors();
    }
}
