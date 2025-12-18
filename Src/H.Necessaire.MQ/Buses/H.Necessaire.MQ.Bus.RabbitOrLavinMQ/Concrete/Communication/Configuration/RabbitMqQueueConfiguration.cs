namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete.Communication.Configuration
{
    internal class RabbitMqQueueConfiguration
    {
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }
        public ushort PrefetchCount { get; set; }
    }
}
