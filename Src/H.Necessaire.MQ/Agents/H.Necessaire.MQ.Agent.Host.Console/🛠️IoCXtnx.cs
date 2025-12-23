using H.Necessaire.MQ.Agent.Core;
using H.Necessaire.Runtime;

namespace H.Necessaire.MQ.Agent.Host.Console
{
    public static class IoCXtnx
    {
        public static T WithHmqConsoleHostAgent<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<DependencyGroup>(() => new DependencyGroup());
            dependencyRegistry.Unregister<SyncRequestProcessingDaemon>();
            return dependencyRegistry;
        }

        public static async Task StartHmqConsoleHostAgent<T>(this T dependencyProvider) where T : ImADependencyProvider
        {
            await dependencyProvider.StartHmqAgent();
        }
    }
}
