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
        }

        public Task<OperationResult<ImAnHmqReActor>[]> Raise(HmqEvent hmqEvent)
        {
            using (IConnection rabbitMqConenction = rabbitMqConnectionFactory.CreateConnection())
            {
                using (IModel rabbitMqChannel = rabbitMqConenction.CreateModel())
                {
                    rabbitMqChannel.ExchangeDeclare(exchange, ExchangeType.Direct);

                    rabbitMqChannel
                        .BasicPublish(
                            exchange: exchange,
                            routingKey: routingKey,
                            basicProperties: null,
                            body: Encoding.UTF8.GetBytes(hmqEvent.ToJsonObject())
                        );
                }
            }

            return OperationResult.Win().WithPayload(RabbitMqReActor.Instance).AsArray().AsTask();
        }
    }
}
