using System;

namespace H.Necessaire.MQ.Bus.RavenDB.Concrete.Storage
{
    internal class RavenDbServiceBusMessageEventArgs : EventArgs
    {
        public RavenDbServiceBusMessageEventArgs(string serviceBusMessageID)
        {
            ServiceBusMessageID = serviceBusMessageID;
        }

        public string ServiceBusMessageID { get; }
    }
}
