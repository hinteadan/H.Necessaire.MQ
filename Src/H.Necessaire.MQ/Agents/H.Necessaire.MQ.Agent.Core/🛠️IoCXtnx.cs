using H.Necessaire.MQ.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Agent.Core
{
    public static class IoCXtnx
    {
        public static T WithHmqAgentCore<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<DependencyGroup>(() => new DependencyGroup());
            return dependencyRegistry;
        }

        public static async Task StartHmqAgent<T>(this T dependencyProvider) where T : ImADependencyProvider
        {
            await dependencyProvider.StartHmqListeners();
            await dependencyProvider.StartDaemons();
        }

        public static async Task StartHmqListeners(this ImADependencyProvider dependencyProvider)
        {
            Type[] listeners = HSafe.Run(() => typeof(ImAnHmqExternalEventListener).GetAllImplementations()).Return();
            if (listeners.IsEmpty())
                return;

            ImAnHmqExternalEventListener[] listenersInstances = listeners.Select(x => dependencyProvider.Get(x) as ImAnHmqExternalEventListener).ToNoNullsArray();

            if (listenersInstances.IsEmpty())
                return;

            if (listenersInstances.Length == 1)
            {
                await HSafe.Run(async () => await listenersInstances[0].Start());
                return;
            }

            foreach (ImAnHmqExternalEventListener instance in listenersInstances)
            {
                if (instance.GetType().GetID() == "PeriodicPolling")
                    continue;

                await HSafe.Run(async () => await instance.Start());
            }
        }

        public static async Task StartDaemons(this ImADependencyProvider dependencyProvider)
        {
            Type[] daemons = HSafe.Run(() => typeof(ImADaemon).GetAllImplementations()).Return();
            if (daemons.IsEmpty())
                return;

            foreach (Type daemon in daemons)
            {
                ImADaemon daemonInstance = dependencyProvider.Get(daemon) as ImADaemon;
                if (daemonInstance is null)
                    continue;

                await HSafe.Run(async () => await daemonInstance.Start());
            }
        }
    }
}
