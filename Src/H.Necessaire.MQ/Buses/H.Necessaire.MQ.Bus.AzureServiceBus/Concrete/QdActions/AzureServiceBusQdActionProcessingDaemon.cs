using Azure.Messaging.ServiceBus;
using H.Necessaire.MQ.Bus.QdActions.Commons;
using H.Necessaire.Serialization;
using System;
using System.Linq;
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
        }

        public override async Task Start(CancellationToken? cancellationToken = null)
        {
            await Task.CompletedTask;

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

            serviceBusClient = new ServiceBusClient(connectionString);
            serviceBusProcessor = serviceBusClient.CreateProcessor(queueName);
            serviceBusProcessor.ProcessMessageAsync += ServiceBusProcessor_ProcessMessageAsync;
            serviceBusProcessor.ProcessErrorAsync += ServiceBusProcessor_ProcessErrorAsync;

            StartListening();
        }

        public override async Task Stop(CancellationToken? cancellationToken = null)
        {
            if (serviceBusClient is null)
                return;

            cancellationTokenSource.Cancel();

            await serviceBusProcessor.StopProcessingAsync();

            await serviceBusProcessor.DisposeAsync();
            await serviceBusClient.DisposeAsync();
        }

        public void Dispose()
        {
            new Action(() =>
            {
                Stop().ConfigureAwait(false).GetAwaiter().GetResult();

            }).TryOrFailWithGrace();
        }

        private async void StartListening()
        {
            await serviceBusProcessor.StartProcessingAsync();
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
    }
}