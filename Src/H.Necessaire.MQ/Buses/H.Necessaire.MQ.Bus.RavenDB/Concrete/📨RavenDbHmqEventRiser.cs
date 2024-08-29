using H.Necessaire.MQ.Abstractions;
using H.Necessaire.MQ.Bus.RavenDB.Concrete.Storage;
using H.Necessaire.MQ.Core;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.RavenDB.Concrete
{
    internal class RavenDbHmqEventRiser : ImAnHmqEventRiser, ImADependency
    {
        RavenDbServiceBusStorageService ravenDbServiceBusStore;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            ravenDbServiceBusStore = dependencyProvider.Get<RavenDbServiceBusStorageService>();
        }

        public async Task<OperationResult<ImAnHmqReActor>[]> Raise(HmqEvent hmqEvent)
        {
            HmqEvent eventToSend = hmqEvent.Clone().MarkAsPersisted();

            await ravenDbServiceBusStore.Save(eventToSend);

            return OperationResult.Win().WithPayload(RavenDbReActor.Instance).AsArray();
        }
    }
}
