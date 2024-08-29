using H.Necessaire.MQ.Abstractions;
using H.Necessaire.MQ.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Concrete
{
    internal class HmqReActor : HmqActorIdentity, ImAnHmqReActor, ImADependency
    {
        internal Func<HmqEvent, Task<OperationResult>> Handler { get; set; }
        internal bool IsHandlingInternalEvents { get; set; } = true;
        internal bool IsHandlingExternalEvents { get; set; } = true;
        internal string[] SpecificHandledSourceIDs { get; set; }
        internal string[] SpecificHandledEventNames { get; set; }
        internal string[] SpecificHandledEventTypes { get; set; }

        ImAnHmqEventReActionRegistry eventReActingRegistry;
        ImALogger logger;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            eventReActingRegistry = dependencyProvider.Get<ImAnHmqEventReActionRegistry>();
            logger = dependencyProvider.GetLogger<HmqReActor>();
        }

        public async Task<OperationResult> Handle(HmqEvent hmqEvent)
        {
            if (!CanHandle(hmqEvent))
                return OperationResult.Win();

            OperationResult result = OperationResult.Fail("Not yet started");

            await
                new Func<Task>(async () =>
                {

                    result = await Handler(hmqEvent) ?? OperationResult.Fail($"Handler for {ID} returned NULL as OperationResult");

                    OperationResult logResult = await eventReActingRegistry.LogEventReAction(hmqEvent, result.WithPayload(this.ToIdentityOnly() as ImAnHmqActorIdentity));
                    if (!logResult.IsSuccessful)
                    {
                        await logger.LogError($"Error occurred while trying to LogEventReAction for {hmqEvent} event in {ID} ReActor");
                    }

                })
                .TryOrFailWithGrace(onFail: ex => result = OperationResult.Fail(ex, $"Error ocurred in {ID} ReActor while handling {hmqEvent.Name} event. Message: {ex.Message}"));

            return result;
        }

        internal bool CanHandle(HmqEvent hmqEvent)
        {
            if (Handler is null)
                return false;

            if (hmqEvent is null)
                return false;

            if (!IsHandlingInternalEvents && !IsHandlingExternalEvents)
                return false;

            if (hmqEvent.IsInternal() && !IsHandlingInternalEvents)
                return false;

            if (hmqEvent.IsExternal() && !IsHandlingExternalEvents)
                return false;

            if (SpecificHandledSourceIDs?.Any() == true && hmqEvent.RaisedByID.NotIn(SpecificHandledSourceIDs))
                return false;

            if (SpecificHandledEventNames?.Any() == true && hmqEvent.Name.NotIn(SpecificHandledEventNames))
                return false;

            if (SpecificHandledEventTypes?.Any() == true && hmqEvent.Type.NotIn(SpecificHandledEventTypes))
                return false;

            return true;
        }
    }
}
