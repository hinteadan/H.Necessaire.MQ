using Azure.Messaging.ServiceBus;
using H.Necessaire.MQ.Abstractions;
using H.Necessaire.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.AzureServiceBus.Concrete
{
    [ID("AzureServiceBus")]
    [Alias("azsb", "az-sb", "azure-sb", "azure-servicebus", "azure-service-bus")]
    internal class AzureServiceBusHmqExternalEventListener : ImAnHmqExternalEventListener, ImADependency, IDisposable
    {
        string connectionString = null;
        string topicName = null;
        string subscriptionName = null;
        ServiceBusClient serviceBusClient = null;
        ServiceBusProcessor serviceBusProcessor = null;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        ImAnHmqEventRiser internalEventRiser;
        ImALogger logger;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            ConfigNode config
                = dependencyProvider
                .GetRuntimeConfig()
                ?.Get("HMQ")
                ?.Get("Azure")
                ?.Get("ServiceBus")
                ;

            connectionString = config?.Get("ConnectionString")?.ToString();
            topicName = config?.Get("TopicName")?.ToString();
            subscriptionName = config?.Get("SubscriptionName")?.ToString();

            internalEventRiser = dependencyProvider.Build<ImAnHmqEventRiser>("internal");

            logger = dependencyProvider.GetLogger<AzureServiceBusHmqExternalEventListener>();
        }

        public async Task<OperationResult> Start()
        {
            await Task.CompletedTask;

            if (connectionString.IsEmpty())
                return OperationResult.Fail("Azure Service Bus connection string is missing. It should be configured @ <ConfigRoot>.HMQ.Azure.ServiceBus.ConnectionString");
            if (topicName.IsEmpty())
                return OperationResult.Fail("Azure Service Bus topic name is missing. It should be configured @ <ConfigRoot>.HMQ.Azure.ServiceBus.TopicName");
            if (subscriptionName.IsEmpty())
                return OperationResult.Fail("Azure Service Bus subscription name is missing. It should be configured @ <ConfigRoot>.HMQ.Azure.ServiceBus.SubscriptionName");

            serviceBusClient = new ServiceBusClient(connectionString);

            serviceBusProcessor = serviceBusClient.CreateProcessor(topicName, subscriptionName);
            serviceBusProcessor.ProcessMessageAsync += ServiceBusProcessor_ProcessMessageAsync;
            serviceBusProcessor.ProcessErrorAsync += ServiceBusProcessor_ProcessErrorAsync;

            StartListening();

            return OperationResult.Win();
        }

        public async Task<OperationResult> Stop()
        {
            if (serviceBusClient is null)
                return OperationResult.Win();

            cancellationTokenSource.Cancel();

            await serviceBusProcessor.StopProcessingAsync();

            await serviceBusProcessor.DisposeAsync();
            await serviceBusClient.DisposeAsync();

            return OperationResult.Win();
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
            string serializedEventReceived = arg.Message.Body.ToString();
            HmqEvent hmqEvent = serializedEventReceived.TryJsonToObject<HmqEvent>().ThrowOnFailOrReturn();
            await internalEventRiser.Raise(hmqEvent);
        }

        private async Task ServiceBusProcessor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            await logger.LogError(arg.Exception);
        }
    }
}
