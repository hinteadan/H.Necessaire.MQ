using System;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.QdActions.Commons
{
    internal abstract class MessageBrokerActionQerBase : ImAnActionQer, ImADependency
    {
        ImAStorageService<Guid, QdAction> qdActionStorageService;
        ImALogger logger;
        public virtual void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            logger = dependencyProvider.GetLogger<MessageBrokerActionQerBase>();
            qdActionStorageService = dependencyProvider.Get<ImAStorageService<Guid, QdAction>>();
        }

        protected abstract Task<OperationResult> QueueActionToMessageBroker(QdAction action);

        public virtual async Task<OperationResult> Queue(QdAction action)
        {
            if (qdActionStorageService is null)
                await logger.LogWarn($"QdAction Storage Service doesn't exist, therefore, even though it might get processed, the QD Action will not be traced");

            await
                new Func<Task>(async () =>
                {
                    (await qdActionStorageService.Save(action.And(a => a.Status = QdActionStatus.Queued))).ThrowOnFail();
                })
                .TryOrFailWithGrace(
                    onFail: async ex =>
                    {
                        string reason = $"Error occurred while trying to store QD Action trace for {action}. Reason: {ex.Message}";
                        await logger.LogError(reason, ex, action);
                    }
                );

            OperationResult result = OperationResult.Fail("Not yet started");

            await
                new Func<Task>(async () =>
                {
                    result = await QueueActionToMessageBroker(action);
                })
                .TryOrFailWithGrace(
                    onFail: async ex =>
                    {
                        string reason = $"Error occurred while trying to queue action to message broker for {action}. Reason: {ex.Message}";
                        await logger.LogError(reason, ex, action);
                        result = OperationResult.Fail(ex, reason);
                    }
                );

            return result;
        }
    }
}
