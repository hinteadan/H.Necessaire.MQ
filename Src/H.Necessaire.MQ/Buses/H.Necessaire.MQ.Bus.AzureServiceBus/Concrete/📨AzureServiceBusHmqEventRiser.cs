using Azure.Messaging.ServiceBus;
using H.Necessaire.MQ.Abstractions;
using H.Necessaire.MQ.Core;
using H.Necessaire.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.AzureServiceBus.Concrete
{
    internal class AzureServiceBusHmqEventRiser : ImAnHmqEventRiser, ImADependency, IDisposable
    {
        string connectionString = null;
        string topicName = null;
        ServiceBusClient serviceBusClient = null;
        ServiceBusSender serviceBusSender = null;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
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

            if (connectionString.IsEmpty())
                return;

            serviceBusClient = new ServiceBusClient(connectionString);
            serviceBusSender = serviceBusClient.CreateSender(topicName);
        }

        public async Task<OperationResult<ImAnHmqReActor>[]> Raise(HmqEvent hmqEvent)
        {
            if (hmqEvent is null)
                return Array.Empty<OperationResult<ImAnHmqReActor>>();

            if (serviceBusSender is null)
                return OperationResult.Fail("Azure Service Bus config is missing. It should be configured @ <ConfigRoot>.HMQ.Azure.ServiceBus.ConnectionString and <ConfigRoot>.HMQ.Azure.ServiceBus.TopicName").WithPayload(AzureServiceBusReActor.Instance).AsArray();

            HmqEvent eventToSend = hmqEvent.Clone().MarkAsPersisted();

            string serializedEventToSend = eventToSend.ToJsonObject();

            if (serializedEventToSend.IsEmpty())
                return OperationResult.Fail("Serialized event is empty").WithPayload(AzureServiceBusReActor.Instance).AsArray();

            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(serializedEventToSend);

            await serviceBusSender.SendMessageAsync(serviceBusMessage, cancellationTokenSource.Token);

            return OperationResult.Win().WithPayload(AzureServiceBusReActor.Instance).AsArray();
        }

        public void Dispose()
        {
            new Action(() =>
            {
                cancellationTokenSource.Cancel();

                if (serviceBusSender != null)
                    serviceBusSender.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                if (serviceBusClient != null)
                    serviceBusClient.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            }).TryOrFailWithGrace();
        }
    }
}
