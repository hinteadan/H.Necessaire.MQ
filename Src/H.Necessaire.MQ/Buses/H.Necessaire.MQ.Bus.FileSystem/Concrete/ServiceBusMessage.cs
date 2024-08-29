using H.Necessaire.MQ.Abstractions;
using System;

namespace H.Necessaire.MQ.Bus.FileSystem.Concrete
{
    internal class ServiceBusMessage : IGuidIdentity
    {
        public Guid ID { get; set; } = Guid.NewGuid();

        public HmqEvent Event { get; set; }

        public static implicit operator ServiceBusMessage(HmqEvent @event) => new ServiceBusMessage { Event = @event };
        public static implicit operator HmqEvent(ServiceBusMessage serviceBusMessage) => serviceBusMessage?.Event;
    }
}
