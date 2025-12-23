using H.Necessaire.MQ.Agent.Core;
using H.Necessaire.Runtime;
using H.Necessaire.Runtime.Integration.DotNet.Daemons;

namespace H.Necessaire.MQ.Agent.Host.AspNetCore
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry.WithHmqAgentCore();

            dependencyRegistry
                .Unregister(typeof(ConsolePingDaemon))
                .Unregister(typeof(SyncRequestProcessingDaemon))
                ;
        }
    }
}
