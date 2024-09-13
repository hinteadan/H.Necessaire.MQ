using Azure.Messaging.ServiceBus;
using H.Necessaire.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.AzureServiceBus.Concrete.QdActions
{
    internal class AzureServiceBusActionQer : ImAnActionQer, ImADependency, IDisposable
    {
        string connectionString = null;
        string queueName = "h-qd-action-queue";
        ServiceBusClient serviceBusClient = null;
        ServiceBusSender serviceBusSender = null;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
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

            if (connectionString.IsEmpty())
                return;

            serviceBusClient = new ServiceBusClient(connectionString);
            serviceBusSender = serviceBusClient.CreateSender(queueOrTopicName: queueName);
        }

        public async Task<OperationResult> Queue(QdAction action)
        {
            if (serviceBusSender is null)
                return OperationResult.Fail("Azure Service Bus config is missing. It should be configured @ <ConfigRoot>.QdActions.Azure.ServiceBus.ConnectionString and <ConfigRoot>.QdActions.Azure.ServiceBus.QueueName");

            string serializedQdActionToSend = action.ToJsonObject();

            if (serializedQdActionToSend.IsEmpty())
                return OperationResult.Fail("Serialized QD Action is empty");

            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(serializedQdActionToSend);

            await serviceBusSender.SendMessageAsync(serviceBusMessage, cancellationTokenSource.Token);

            return OperationResult.Win();
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