using Azure.Messaging.ServiceBus;
using H.Necessaire.MQ.Bus.Commons;
using H.Necessaire.MQ.Bus.QdActions.Commons;
using H.Necessaire.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.AzureServiceBus.Concrete.QdActions
{
    [ID("AzureServiceBus")]
    [Alias("azsb", "az-sb", "azure-sb", "azure-servicebus", "azure-service-bus")]
    internal class AzureServiceBusQdActionProcessingDaemon : MessageBrokerNotifiedQdActionProcessingDaemonBase, IDisposable
    {
        string connectionString = null;
        string queueName = "h-qd-action-queue";
        ServiceBusClient serviceBusClient = null;
        ServiceBusProcessor serviceBusProcessor = null;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        ImALogger logger;
        ImAResilienceRecoveryRegistry resilienceRecoveryRegistry;
        bool isListening = false;
        public override void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            base.ReferDependencies(dependencyProvider);

            logger = dependencyProvider.GetLogger<AzureServiceBusQdActionProcessingDaemon>();

            ConfigNode config
                = dependencyProvider
                .GetRuntimeConfig()
                ?.Get("QdActions")
                ?.Get("Azure")
                ?.Get("ServiceBus")
                ;

            string queueNameFromConfig = config?.Get("QueueName")?.ToString();
            queueName = !queueNameFromConfig.IsEmpty() ? queueNameFromConfig : queueName;

            string connectionStringFromConfig = config?.Get("ConnectionString")?.ToString();
            connectionString = !connectionStringFromConfig.IsEmpty() ? connectionStringFromConfig : connectionString;

            uint? maxConcurrentCallsFromConfig = config?.Get("MaxConcurrentCalls")?.ToString()?.ParseToUIntOrFallbackTo(null);
            maxConcurrentMessageHandling = (maxConcurrentCallsFromConfig == null) ? maxConcurrentMessageHandling : (ushort)maxConcurrentCallsFromConfig.Value;

            resilienceRecoveryRegistry = dependencyProvider.Get<ImAResilienceRecoveryRegistry>();
        }

        public override async Task Start(CancellationToken? cancellationToken = null)
        {
            if (connectionString.IsEmpty())
            {
                await logger.LogError("Azure Service Bus connection string is missing. It should be configured @ <ConfigRoot>.QdActions.Azure.ServiceBus.ConnectionString");
                return;
            }
            if (queueName.IsEmpty())
            {
                await logger.LogError("Azure Service Bus queue name is missing. It should be configured @ <ConfigRoot>.QdActions.Azure.ServiceBus.QueueName");
                return;
            }

            int retryAttempt = 0;
            await new Func<Task>(async () =>
            {

                serviceBusClient = new ServiceBusClient(connectionString);
                serviceBusProcessor = serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions { AutoCompleteMessages = false, MaxConcurrentCalls = maxConcurrentMessageHandling });
                serviceBusProcessor.ProcessMessageAsync += ServiceBusProcessor_ProcessMessageAsync;
                serviceBusProcessor.ProcessErrorAsync += ServiceBusProcessor_ProcessErrorAsync;

                await StartListening();

                isListening = true;

            })
            .TryOrFailWithGrace(
                numberOfTimes: 2,
                onFail: async ex =>
                {
                    await logger.LogError($"Error occurred while trying to connect to Azure Service Bus after {retryAttempt + 1} attempt(s). Reason: {ex.Message}", ex);
                    await Stop(cancellationToken);
                },
                onRetry: async ex =>
                {
                    await logger.LogWarn($"Couldn't connect to Azure Service Bus, retrying (attempt {++retryAttempt})... Reason: {ex.Message}", ex);
                    await Stop(cancellationToken);
                },
                millisecondsToSleepBetweenRetries: 1000
            );

            resilienceRecoveryRegistry.RegisterResilienceTask(RunResiliencyChecks);
        }

        public override async Task Stop(CancellationToken? cancellationToken = null)
        {
            resilienceRecoveryRegistry.UnregisterResilienceTask(RunResiliencyChecks);

            isListening = false;

            if (serviceBusClient is null)
                return;

            new Action(() =>
            {

                serviceBusProcessor.ProcessMessageAsync -= ServiceBusProcessor_ProcessMessageAsync;
                serviceBusProcessor.ProcessErrorAsync -= ServiceBusProcessor_ProcessErrorAsync;

            }).TryOrFailWithGrace();

            new Action(cancellationTokenSource.Cancel).TryOrFailWithGrace();

            await new Func<Task>(async () => await serviceBusProcessor.StopProcessingAsync()).TryOrFailWithGrace(onFail: ex => { });

            await new Func<Task>(async () => await serviceBusProcessor.DisposeAsync()).TryOrFailWithGrace(onFail: ex => { });
            await new Func<Task>(async () => await serviceBusClient.DisposeAsync()).TryOrFailWithGrace(onFail: ex => { });
        }

        public void Dispose()
        {
            new Action(() =>
            {
                Stop().ConfigureAwait(false).GetAwaiter().GetResult();

            }).TryOrFailWithGrace();
        }

        private async Task StartListening()
        {
            await serviceBusProcessor.StartProcessingAsync(cancellationTokenSource.Token).ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task ServiceBusProcessor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            string qdActionAsJsonString = arg.Message.Body.ToString();
            QdAction qdAction = qdActionAsJsonString.TryJsonToObject<QdAction>().ThrowOnFailOrReturn();

            await ProcessQdActionProcessingResult(
                await ProcessQdAction(qdAction),
                failMarker: async () => await arg.CompleteMessageAsync(arg.Message, cancellationTokenSource.Token),
                winMarker: async () => await arg.CompleteMessageAsync(arg.Message, cancellationTokenSource.Token)
            );
        }

        private async Task ServiceBusProcessor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            await logger.LogError(arg.Exception);
        }

        private async Task RunResiliencyChecks()
        {
            await logger.LogTrace($"Running Azure Service Bus Resiliency Checks");
            TimeSpan duration = TimeSpan.Zero;
            using (new TimeMeasurement(x => duration = x))
            {
                if (serviceBusClient != null && serviceBusProcessor != null && isListening)
                    return;

                await Stop();
                await Start();
            }
            await logger.LogTrace($"DONE Running Azure Service Bus Resiliency Checks in {duration}");
        }
    }
}