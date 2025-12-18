using H.Necessaire.MQ.Abstractions;
using H.Necessaire.Serialization;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete
{
    internal class RabbitMqHmqEventRiser : ImAnHmqEventRiser, ImADependency
    {
        string exchange = "hmq";
        string routingKey = "hmq";
        ConnectionFactory rabbitMqConnectionFactory;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            ConfigNode config
                = dependencyProvider
                .GetRuntimeConfig()
                ?.Get("HMQ")
                ?.Get("RabbitMQ")
                ;

            rabbitMqConnectionFactory = new ConnectionFactory
            {
                HostName = config?.Get("HostName")?.ToString(),
                VirtualHost = config?.Get("VirtualHost")?.ToString(),
                UserName = config?.Get("UserName")?.ToString(),
                Password = config?.Get("Password")?.ToString(),
            };

            string routingKeyFromConfig = config?.Get("RoutingKey")?.ToString();
            routingKey = !routingKeyFromConfig.IsEmpty() ? routingKeyFromConfig : routingKey;

            string exchangeFromConfig = config?.Get("Exchange")?.ToString();
            exchange = !exchangeFromConfig.IsEmpty() ? exchangeFromConfig : exchange;
        }

        public async Task<OperationResult<ImAnHmqReActor>[]> Raise(HmqEvent hmqEvent)
        {
            using (IConnection rabbitMqConenction = await rabbitMqConnectionFactory.CreateConnectionAsync())
            {
                using (IChannel rabbitMqChannel = await rabbitMqConenction.CreateChannelAsync())
                {
                    await rabbitMqChannel.ExchangeDeclareAsync(exchange, ExchangeType.Direct);

                    await rabbitMqChannel
                        .BasicPublishAsync(
                            exchange: exchange,
                            routingKey: routingKey,
                            body: Encoding.UTF8.GetBytes(hmqEvent.ToJsonObject())
                        );
                }
            }

            return OperationResult.Win().WithPayload(RabbitMqReActor.Instance).AsArray();
        }
    }
}
