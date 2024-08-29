using H.Necessaire.MQ.Abstractions;
using System;

namespace H.Necessaire.MQ
{
    internal interface ImAnHmqActorAndReActorBookkeeper
    {
        ImAnHmqActor GetOrAddActor(string id, Func<string, ImAnHmqActor> actorBuilder);
        ImAnHmqReActor GetOrAddReActor(string id, Func<string, ImAnHmqReActor> reActorBuilder);
        ImAnHmqReActor[] GetAllReActors();
    }
}
