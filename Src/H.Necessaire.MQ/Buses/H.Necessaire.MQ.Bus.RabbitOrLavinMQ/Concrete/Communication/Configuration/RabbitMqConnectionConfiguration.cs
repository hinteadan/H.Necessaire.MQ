namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete.Communication.Configuration
{
    internal class RabbitMqConnectionConfiguration
    {
        public string HostName { get; set; }
        public string VirtualHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
