using H.Necessaire.MQ.Abstractions;
using H.Necessaire.MQ.Core;
using System.Linq;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Concrete
{
    internal class HmqActor : HmqActorIdentity, ImAnHmqActor, ImADependency
    {
        ImAnHmqEventRegistry eventRegistry;
        ImAnHmqEventRiser eventRiser;
        ImAnHmqEventRiser internalEventRiser;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            eventRegistry = dependencyProvider.Get<ImAnHmqEventRegistry>();
            eventRiser = dependencyProvider.Get<ImAnHmqEventRiser>();
            internalEventRiser = dependencyProvider.Build<ImAnHmqEventRiser>("internal");
        }

        public async Task<OperationResult> Raise(HmqEvent hmqEvent)
        {
            hmqEvent = hmqEvent.Clone().And(x => x.RaisedBy = this.ToIdentityOnly());

            OperationResult appendResult = await eventRegistry.Append(hmqEvent);

            if (!appendResult.IsSuccessful)
                return appendResult;

            OperationResult<ImAnHmqReActor>[] raiseResults = await internalEventRiser.Raise(hmqEvent);
            if (eventRiser != internalEventRiser)
            {
                OperationResult<ImAnHmqReActor>[] externalRaiseResults = await eventRiser.Raise(hmqEvent.Clone().MarkAsPersisted());
                raiseResults = raiseResults.Push(externalRaiseResults);
            }

            OperationResult<OperationResult<ImAnHmqReActor>[]> globalRaiseResult = raiseResults.Merge(globalReasonIfNecesarry: "Some of the HMQ ReActors failed to handle the event. Check payload for details.").WithPayload(raiseResults.Where(x => !x.IsSuccessful).ToArrayNullIfEmpty());

            if (!globalRaiseResult.IsSuccessful)
                await HandleRaiseFailures(hmqEvent, globalRaiseResult.Payload.Select(x => x.Payload).ToArray());

            return globalRaiseResult;
        }

#pragma warning disable IDE0060 // Remove unused parameter
        private Task HandleRaiseFailures(HmqEvent hmqEvent, ImAnHmqReActor[] imAnHmqReActors)
        {
            //TODO: Handle raise failures if necessary
            return Task.CompletedTask;
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
