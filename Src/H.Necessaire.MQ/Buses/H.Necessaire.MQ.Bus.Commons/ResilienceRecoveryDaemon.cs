using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.Commons
{
    internal class ResilienceRecoveryDaemon : ImADaemon, ImADependency
    {
#if DEBUG
        TimeSpan processingInterval = TimeSpan.FromSeconds(5);
#else
        TimeSpan processingInterval = TimeSpan.FromMinutes(5);
#endif
        ImAPeriodicAction resilienceTasksExecutionPeriodicAction;
        ImALogger logger;
        readonly ConcurrentBag<Func<Task>> resilienceTasks = new ConcurrentBag<Func<Task>>();

        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            resilienceTasksExecutionPeriodicAction = dependencyProvider.Get<ImAPeriodicAction>();
            logger = dependencyProvider.GetLogger<ResilienceRecoveryDaemon>();
        }

        public Task Start(CancellationToken? cancellationToken = null)
        {
            resilienceTasksExecutionPeriodicAction.StartDelayed(processingInterval, processingInterval, RunProcessingSession);
            return true.AsTask();
        }

        public Task Stop(CancellationToken? cancellationToken = null)
        {
            resilienceTasksExecutionPeriodicAction.Stop();
            return true.AsTask();
        }

        async Task RunProcessingSession()
        {
            int resilienceTasksCount = resilienceTasks.Count;

            if (resilienceTasksCount == 0)
                return;

            await logger.LogTrace($"Running ALL {resilienceTasksCount} resilience recovery tasks");
            TimeSpan duration = TimeSpan.Zero;
            using (new TimeMeasurement(x => duration = x))
            {
                await
                    Task.WhenAll(
                        resilienceTasks.Select(RunTask)
                    );
            }
            await logger.LogTrace($"DONE Running ALL {resilienceTasksCount} resilience recovery tasks in {duration}");
        }

        async Task RunTask(Func<Task> task, int index)
        {
            await
                new Func<Task>(async () =>
                {
                    await logger.LogTrace($"Running resilience recovery task #{index + 1}");
                    TimeSpan duration = TimeSpan.Zero;
                    using (new TimeMeasurement(x => duration = x))
                    {
                        await task.Invoke();
                    }
                    await logger.LogTrace($"DONE Running resilience recovery task #{index + 1} in {duration}");
                })
                .TryOrFailWithGrace(
                    onFail: async ex =>
                    {
                        string reason = $"Error occurred while trying to run a resilience recovery task #{index + 1}. Reason: {ex.Message}";
                        await logger.LogError(reason, ex);
                    }
                );
        }
    }
}
