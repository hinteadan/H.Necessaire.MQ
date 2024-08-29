using H.Necessaire.MQ.Core;

namespace H.Necessaire.MQ.Concrete
{
    [ID("InternalEventRegistry")]
    [Alias("internal", "internal-event-registry")]
    internal class HmqEventRegistry : HmqEventRegistryBackedByStorageServicesBase
    {

    }
}
