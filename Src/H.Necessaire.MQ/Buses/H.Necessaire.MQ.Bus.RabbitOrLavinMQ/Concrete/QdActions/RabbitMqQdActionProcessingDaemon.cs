﻿using H.Necessaire.MQ.Bus.QdActions.Commons;
using H.Necessaire.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete.QdActions
{
    [ID("RabbitMq")]
    [Alias("rabbit-mq", "rabbit", "lavin-mq", "lavin")]
    internal class RabbitMqQdActionProcessingDaemon : MessageBrokerNotifiedQdActionProcessingDaemonBase
    {
        const ushort optimalNumberOfProcessingThreadsPerCpu = 8;
        ushort prefetchCount = (ushort)(optimalNumberOfProcessingThreadsPerCpu * Environment.ProcessorCount);
        string queueName = "h-qd-action-queue";
        string routingKey = "h-qd-action-queue";
        ConnectionFactory rabbitMqConnectionFactory;
        IConnection rabbitMqConnection;
        IModel rabbitMqChannel;
        EventingBasicConsumer eventConsumer;

        public override void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            base.ReferDependencies(dependencyProvider);

            ConfigNode config
                = dependencyProvider
                .GetRuntimeConfig()
                ?.Get("QdActions")
                ?.Get("RabbitMQ")
                ;

            rabbitMqConnectionFactory = new ConnectionFactory
            {
                HostName = config?.Get("HostName")?.ToString(),
                VirtualHost = config?.Get("VirtualHost")?.ToString(),
                UserName = config?.Get("UserName")?.ToString(),
                Password = config?.Get("Password")?.ToString(),
            };

            string queueNameFromConfig = config?.Get("QueueName")?.ToString();
            queueName = !queueNameFromConfig.IsEmpty() ? queueNameFromConfig : queueName;

            string routingKeyFromConfig = config?.Get("RoutingKey")?.ToString();
            routingKey = !routingKeyFromConfig.IsEmpty() ? routingKeyFromConfig : routingKey;

            uint? prefetchCountFromConfig = config?.Get("PrefetchCount")?.ToString()?.ParseToUIntOrFallbackTo(null);
            prefetchCount = (prefetchCountFromConfig == null) ? prefetchCount : (ushort)prefetchCountFromConfig;
        }

        public override Task Start(CancellationToken? cancellationToken = null)
        {
            rabbitMqConnection = rabbitMqConnectionFactory.CreateConnection();
            rabbitMqChannel = rabbitMqConnection.CreateModel();
            QueueDeclareOk queue = rabbitMqChannel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: prefetchCount, global: false);

            eventConsumer = new EventingBasicConsumer(rabbitMqChannel);

            eventConsumer.Received += EventConsumer_Received;

            rabbitMqChannel
                .BasicConsume(
                    queue: queue.QueueName,
                    autoAck: false,
                    consumer: eventConsumer
                );

            return true.AsTask();
        }

        public override Task Stop(CancellationToken? cancellationToken = null)
        {
            eventConsumer.Received -= EventConsumer_Received;
            eventConsumer = null;
            rabbitMqChannel.Dispose();
            rabbitMqConnection.Dispose();
            return true.AsTask();
        }

        private async void EventConsumer_Received(object sender, BasicDeliverEventArgs args)
        {
            byte[] body = args.Body.ToArray();

            string qdActionAsJsonString = Encoding.UTF8.GetString(body);

            QdAction qdAction = qdActionAsJsonString.TryJsonToObject<QdAction>().ThrowOnFailOrReturn();

            await ProcessQdActionProcessingResult(
                await ProcessQdAction(qdAction), 
                failMarker: () => { rabbitMqChannel.BasicNack(deliveryTag: args.DeliveryTag, multiple: false, requeue: false); return true.AsTask(); },
                winMarker: () => { rabbitMqChannel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false); return true.AsTask(); }
            );
        }
    }
}
