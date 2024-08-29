using H.Necessaire.MQ.Abstractions;
using System;

namespace H.Necessaire.MQ.Bus.RavenDB.Concrete
{
    internal class ServiceBusMessage : IStringIdentity
    {
        public string ID { get; set; } = $"ServiceBusMessage/{Guid.NewGuid()}";

        public HmqEvent Event { get; set; }

        public static implicit operator ServiceBusMessage(HmqEvent @event) => new ServiceBusMessage { Event = @event };
        public static implicit operator HmqEvent(ServiceBusMessage serviceBusMessage) => serviceBusMessage?.Event;
    }
}
