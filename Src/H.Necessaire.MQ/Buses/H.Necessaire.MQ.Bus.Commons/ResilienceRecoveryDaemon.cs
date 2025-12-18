using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.Commons
{
    [Alias("ResilienceRecovery", "resilience-recovery")]
    internal class ResilienceRecoveryDaemon : ImAResilienceRecoveryRegistry, ImADaemon, ImADependency
    {
#if DEBUG
        TimeSpan processingInterval = TimeSpan.FromSeconds(5);
#else
        TimeSpan processingInterval = TimeSpan.FromMinutes(5);
#endif
        ImAPeriodicAction resilienceTasksExecutionPeriodicAction;
        ImALogger logger;
        readonly ConcurrentDictionary<string, Func<Task>> resilienceTasks = new ConcurrentDictionary<string, Func<Task>>();

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
                        resilienceTasks.Values.Select(RunTask)
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

        public void RegisterResilienceTask(Func<Task> resilienceTask)
        {
            if (resilienceTask is null)
                return;

            resilienceTasks.TryAdd(BuildID(resilienceTask), resilienceTask);
        }

        public void UnregisterResilienceTask(Func<Task> resilienceTask)
        {
            if (resilienceTask is null)
                return;

            resilienceTasks.TryRemove(BuildID(resilienceTask), out Func<Task> removedResilienceTask);
        }

        public IEnumerable<Func<Task>> StreamAll() => resilienceTasks.Values.AsEnumerable();

        static string BuildID(Func<Task> resilienceTask)
        {
            return $"{resilienceTask.Method.DeclaringType.FullName}.{resilienceTask.Method.Name}";
        }
    }
}
