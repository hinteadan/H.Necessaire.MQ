using H.Necessaire.MQ.Abstractions;
using H.Necessaire.MQ.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Concrete
{
    [ID("InternalEventRiser")]
    [Alias("internal", "internal-event-riser")]
    internal class HmqEventInternalRiser : ImAnHmqEventRiser, ImADependency
    {
        static readonly TimeSpan eventRetryMinIterval = TimeSpan.FromSeconds(5);
        static readonly TimeSpan eventRetryMaxIterval = TimeSpan.FromMinutes(5);
        static readonly TimeSpan eventRetryIncrement = TimeSpan.FromSeconds(5);

        ImAnHmqActorAndReActorBookkeeper actorAndReActorBookkeeper;
        ImAnHmqEventReActionRegistry eventReActionRegistry;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            actorAndReActorBookkeeper = dependencyProvider.Get<ImAnHmqActorAndReActorBookkeeper>();
            eventReActionRegistry = dependencyProvider.Get<ImAnHmqEventReActionRegistry>();
        }

        public async Task<OperationResult<ImAnHmqReActor>[]> Raise(HmqEvent hmqEvent)
        {
            if (hmqEvent is null)
                return Array.Empty<OperationResult<ImAnHmqReActor>>();

            ImAnHmqReActor[] allKnownReactors = actorAndReActorBookkeeper.GetAllReActors();

            ImAnHmqReActor[] candidateReactors
                = allKnownReactors
                .Where(x => (x as HmqReActor)?.CanHandle(hmqEvent) != false)
                .ToArrayNullIfEmpty()
                ;

            if (candidateReactors?.Any() != true)
                return Array.Empty<OperationResult<ImAnHmqReActor>>();

            ImAnHmqActorIdentity[] reactorsToIgnore = await DetermineReactorsToIgnore(hmqEvent);
            string[] reactorIDsToIgnore = reactorsToIgnore?.Select(x => x.ID.NullIfEmpty()).ToNoNullsArray();

            ImAnHmqReActor[] reActorsToRaise = candidateReactors;
            if (reactorIDsToIgnore?.Any() == true)
            {
                reActorsToRaise
                    = reActorsToRaise
                    .Where(x => x.ID.NotIn(reactorIDsToIgnore))
                    .ToArrayNullIfEmpty();
            }

            if (reActorsToRaise?.Any() != true)
                return Array.Empty<OperationResult<ImAnHmqReActor>>();

            OperationResult<ImAnHmqReActor>[] results = await Task.WhenAll(reActorsToRaise.Select(r => Raise(hmqEvent, r)));

            return results;
        }

        async Task<ImAnHmqActorIdentity[]> DetermineReactorsToIgnore(HmqEvent hmqEvent)
        {
            HmqEventReactionLog[] eventReactionLogs = await LoadReactionLogsFor(hmqEvent);

            if (eventReactionLogs?.Any() != true)
                return null;

            return
                eventReactionLogs
                .GroupBy(x => x.ActorID)
                .Where(IsReActorIgnored)
                .Select(g => g.First().ActorIdentity)
                .ToNoNullsArray()
                ;
        }

        bool IsReActorIgnored(IGrouping<string, HmqEventReactionLog> reActorLogs)
        {
            HmqEventReactionLog latestLog = reActorLogs.OrderByDescending(x => x.AsOf).First();
            if (latestLog.IsSuccessful)
                return true;

            int numberOfAttempts = reActorLogs.Count();
            TimeSpan retryCoolDown = TimeSpan.FromSeconds(numberOfAttempts * eventRetryIncrement.TotalSeconds);
            if (retryCoolDown < eventRetryMinIterval)
                retryCoolDown = eventRetryMinIterval;
            if (retryCoolDown > eventRetryMaxIterval)
                retryCoolDown = eventRetryMaxIterval;
            DateTime timestampToResumeRetry = latestLog.AsOf + retryCoolDown;

            bool canRetry = DateTime.UtcNow >= timestampToResumeRetry;

            if (!canRetry)
                return true;

            return false;
        }

        async Task<HmqEventReactionLog[]> LoadReactionLogsFor(HmqEvent hmqEvent)
        {
            OperationResult<IDisposableEnumerable<HmqEventReactionLog>> reActorsStreamResult
                = await eventReActionRegistry.Stream(new HmqEventReActionFilter
                {
                    EventIDs = hmqEvent.ID.AsArray(),
                });

            if (!reActorsStreamResult.IsSuccessful)
                return null;

            using (reActorsStreamResult.Payload)
            {
                return reActorsStreamResult.Payload.ToArray().NullIfEmpty();
            }
        }

        async Task<OperationResult<ImAnHmqReActor>> Raise(HmqEvent hmqEvent, ImAnHmqReActor reactor)
        {
            OperationResult<ImAnHmqReActor> result = OperationResult.Fail("Not yet started").WithPayload(reactor);

            await
                new Func<Task>(async () =>
                {
                    result = (await reactor.Handle(hmqEvent.ToWellTypedEventData())).WithPayload(reactor);
                })
                .TryOrFailWithGrace(
                    onFail: ex => result = OperationResult.Fail(ex, $"Error occurred while trying to handle {hmqEvent.Name}({hmqEvent.ID}) that happened on {hmqEvent.HappenedAt.PrintDateAndTime()}. Message: {ex.Message}").WithPayload(reactor)
                );

            return result;
        }
    }
}
