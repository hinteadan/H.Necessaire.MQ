using H.Necessaire.MQ.Abstractions;
using H.Necessaire.MQ.Concrete;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace H.Necessaire.MQ
{
    public static class HmqActorAndReActorExtensions
    {
        public static ImAnHmqActor GetHmqActor(this ImADependencyProvider dependencyProvider, string id, params Note[] idAttrs)
        {
            if (id.IsEmpty())
                throw new ArgumentException($"HMQ Actor ID must be specified", nameof(id));

            ImAnHmqActorAndReActorBookkeeper actorAndReActorBookkeeper = dependencyProvider.Get<ImAnHmqActorAndReActorBookkeeper>();

            ImAnHmqActor actor = actorAndReActorBookkeeper.GetOrAddActor(id, i => dependencyProvider.Get<HmqActor>().And(a =>
            {
                a.ID = i;
                a.IdentityAttributes = idAttrs?.Where(x => !x.IsEmpty())?.ToArrayNullIfEmpty();
            }));

            return actor;
        }

        public static ImAnHmqReActor GetCatchAllHmqReActor(this ImADependencyProvider dependencyProvider, Func<HmqEvent, Task<OperationResult>> handler, string id, params Note[] idAttrs)
            => dependencyProvider.GetHmqReActor(handler, id, idAttrs);

        public static ImAnHmqReActor GetCatchAllInternalHmqReActor(this ImADependencyProvider dependencyProvider, Func<HmqEvent, Task<OperationResult>> handler, string id, params Note[] idAttrs)
            => dependencyProvider.GetHmqReActor(handler, id, idAttrs, x => x.IsHandlingExternalEvents = false);

        public static ImAnHmqReActor GetCatchAllExternalHmqReActor(this ImADependencyProvider dependencyProvider, Func<HmqEvent, Task<OperationResult>> handler, string id, params Note[] idAttrs)
            => dependencyProvider.GetHmqReActor(handler, id, idAttrs, x => x.IsHandlingInternalEvents = false);


        public static ImAnHmqReActor GetTargetedHmqReActor(this ImADependencyProvider dependencyProvider, Func<HmqEvent, Task<OperationResult>> handler, string id, Note[] idAttrs, string[] sourceIDs, string[] eventNames, string[] eventTypes)
            => dependencyProvider.GetHmqReActor(handler, id, idAttrs, x =>
            {
                x.SpecificHandledSourceIDs = sourceIDs;
                x.SpecificHandledEventNames = eventNames;
                x.SpecificHandledEventTypes = eventTypes;
            });

        public static ImAnHmqReActor GetTargetedInternalHmqReActor(this ImADependencyProvider dependencyProvider, Func<HmqEvent, Task<OperationResult>> handler, string id, Note[] idAttrs, string[] sourceIDs, string[] eventNames, string[] eventTypes)
            => dependencyProvider.GetHmqReActor(handler, id, idAttrs, x =>
            {
                x.IsHandlingExternalEvents = false;
                x.SpecificHandledSourceIDs = sourceIDs;
                x.SpecificHandledEventNames = eventNames;
                x.SpecificHandledEventTypes = eventTypes;
            });

        public static ImAnHmqReActor GetTargetedExternalHmqReActor(this ImADependencyProvider dependencyProvider, Func<HmqEvent, Task<OperationResult>> handler, string id, Note[] idAttrs, string[] sourceIDs, string[] eventNames, string[] eventTypes)
            => dependencyProvider.GetHmqReActor(handler, id, idAttrs, x =>
            {
                x.IsHandlingInternalEvents = false;
                x.SpecificHandledSourceIDs = sourceIDs;
                x.SpecificHandledEventNames = eventNames;
                x.SpecificHandledEventTypes = eventTypes;
            });


        private static ImAnHmqReActor GetHmqReActor(this ImADependencyProvider dependencyProvider, Func<HmqEvent, Task<OperationResult>> handler, string id, Note[] idAttrs, Action<HmqReActor> config = null)
        {
            if (id.IsEmpty())
                throw new ArgumentException($"HMQ ReActor ID must be specified", nameof(id));

            ImAnHmqActorAndReActorBookkeeper actorAndReActorBookkeeper = dependencyProvider.Get<ImAnHmqActorAndReActorBookkeeper>();

            ImAnHmqReActor reActor = actorAndReActorBookkeeper.GetOrAddReActor(id, i => dependencyProvider.Get<HmqReActor>().And(r =>
            {
                r.ID = i;
                r.IdentityAttributes = idAttrs?.Where(x => !x.IsEmpty())?.ToArrayNullIfEmpty();
                r.Handler = handler;
                if (config != null)
                    config(r);
            }));

            return reActor;
        }
    }
}
