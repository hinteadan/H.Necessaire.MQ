﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.QdActions.Commons
{
    internal abstract class MessageBrokerNotifiedQdActionProcessingDaemonBase : ImADaemon, ImAQdActionQueueOnDemandRunner, ImADependency
    {
        ImALogger logger;
        ImAQdActionProcessor[] allKnownProcessors;
        ImAStorageService<Guid, QdActionResult> qdActionResultStorage;
        ImAStorageService<Guid, QdAction> qdActionStorageService;
        ImAnActionQer actionQer;
        int maxProcessingAttempts = 3;
        public virtual void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            maxProcessingAttempts = dependencyProvider.GetRuntimeConfig()?.Get("QdActions")?.Get("MaxProcessingAttempts")?.ToString()?.ParseToIntOrFallbackTo(3) ?? 3;
            allKnownProcessors = typeof(ImAQdActionProcessor).GetAllImplementations().Select(t => dependencyProvider.Get(t) as ImAQdActionProcessor).Where(x => x != null).ToArray();

            logger = dependencyProvider.GetLogger<MessageBrokerNotifiedQdActionProcessingDaemonBase>();
            qdActionResultStorage = dependencyProvider.Get<ImAStorageService<Guid, QdActionResult>>();
            qdActionStorageService = dependencyProvider.Get<ImAStorageService<Guid, QdAction>>();
            actionQer = dependencyProvider.Get<ImAnActionQer>();
        }

        public abstract Task Start(CancellationToken? cancellationToken = null);

        public abstract Task Stop(CancellationToken? cancellationToken = null);

        public virtual Task<OperationResult<QdActionResult[]>> RunQdActionQueueProcessingCycle()
        {
            return OperationResult.Win().WithoutPayload<QdActionResult[]>().AsTask();
        }

        protected async Task<QdActionResult> ProcessQdAction(QdAction qdAction)
        {
            QdActionResult result = OperationResult.Fail("Not yet started").WithPayload(qdAction).ToQdActionResult();

            await UpdateQdActionState(qdAction.And(q => q.Status = QdActionStatus.Running));

            await
                new Func<Task>(async () =>
                {
                    await logger.LogTrace($"Processing QdAction {qdAction}");
                    using (new TimeMeasurement(async x => await logger.LogTrace($"DONE Processing QdAction {qdAction} in {x}")))
                    {
                        QdActionResult processingResult = await RunEligibleProcessorForQdAction(qdAction);

                        await UpdateQdActionState(
                            qdAction
                                .And(q => q.RunCount++)
                                .And(q => q.Status = processingResult.IsSuccessful ? QdActionStatus.Succeeded : QdActionStatus.Failed)
                        );

                        if (qdActionResultStorage != null)
                        {
                            await qdActionResultStorage.Save(processingResult);
                        }

                        result = processingResult;
                    }
                })
                .TryOrFailWithGrace(
                    onFail: async ex =>
                    {
                        await logger.LogError(ex);
                        result = OperationResult.Fail(ex, $"Error occurred while processing QD Action {qdAction}. Message: {ex.Message}").WithPayload(qdAction).ToQdActionResult();
                    }
                );

            await UpdateQdActionState(qdAction.And(q => q.Status = result.IsSuccessful ? QdActionStatus.Succeeded : QdActionStatus.Failed));

            return result;
        }

        protected async Task ProcessQdActionProcessingResult(QdActionResult processingResult, Func<Task> failMarker, Func<Task> winMarker)
        {
            if (!processingResult.IsSuccessful)
            {
                await failMarker();

                QdAction qdAction = processingResult.Payload;

                bool shouldReQueue = qdAction.RunCount < maxProcessingAttempts;

                if (!shouldReQueue)
                    return;

                OperationResult requeueResult = await actionQer.Queue(qdAction);
                if (!requeueResult.IsSuccessful)
                {
                    await logger.LogError($"Error occurred while trying to requeue QD Action {qdAction}. Reason: {requeueResult.Reason}", new OperationResultException(requeueResult), qdAction, requeueResult.FlattenReasons().ToNotes("Reason"));
                }

                return;
            }

            await winMarker();
        }

        private async Task<QdActionResult> RunEligibleProcessorForQdAction(QdAction qdAction)
        {
            QdActionResult result = OperationResult.Fail("Not yet started").WithPayload(qdAction).ToQdActionResult();

            await
                new Func<Task>(async () =>
                {
                    ImAQdActionProcessor qdActionProcessor = await GetProcessorFor(qdAction);
                    if (qdActionProcessor == null)
                    {
                        result = OperationResult.Fail($"No eligible QD Action Processor found for {qdAction}").WithPayload(qdAction).ToQdActionResult();
                        return;
                    }

                    result = await qdActionProcessor.Process(qdAction);
                })
                .TryOrFailWithGrace(
                    onFail: async ex =>
                    {
                        await logger.LogError(ex);
                        result = OperationResult.Fail(ex).WithPayload(qdAction).ToQdActionResult();
                    }
                );

            return DecorateWithRuntimeInfo(result);
        }

        private async Task<ImAQdActionProcessor> GetProcessorFor(QdAction qdAction)
        {
            if (!allKnownProcessors?.Any() ?? true)
                return null;

            foreach (ImAQdActionProcessor processor in allKnownProcessors)
            {
                if (await processor.IsEligibleFor(qdAction))
                    return processor;
            }

            return null;
        }

        private static QdActionResult DecorateWithRuntimeInfo(QdActionResult qdActionResult)
        {
            return
                qdActionResult
                ?.And(x =>
                {
                    x.Comments = x.Comments.Push(Note.GetEnvironmentInfo().AppendProcessInfo().Select(p => p.ToString()).ToArray(), checkDistinct: false);
                });
        }

        private async Task UpdateQdActionState(QdAction action)
        {
            if (qdActionStorageService is null)
                return;

            await
                new Func<Task>(async () =>
                {
                    (await qdActionStorageService.Save(action)).ThrowOnFail();
                })
                .TryOrFailWithGrace(
                    onFail: async ex =>
                    {
                        string reason = $"Error occurred while trying to Update QdAction State for {action}. Reason: {ex.Message}";
                        await logger.LogError(reason, ex, action);
                    }
                );
        }
    }
}
