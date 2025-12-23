using H.Necessaire.MQ.Agent.Host.AspNetCore;
using H.Necessaire.MQ.Bus.RabbitOrLavinMQ;
using H.Necessaire.MQ.Runtime.RavenDb;
using H.Necessaire.Operations.Versioning.Concrete;
using H.Necessaire.Runtime.RavenDB;

namespace H.Necessaire.MQ.Agent.Sample.AspNetCore
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .WithHmqAspNetHostAgent()
                .Register<ImAVersionProvider>(() => new EmbeddedResourceVersionProvider(typeof(DependencyGroup).Assembly))
                .Unregister<QdActionProcessingDaemon>()
                .Register<RavenDbRuntimeDependencyGroup>(() => new RavenDbRuntimeDependencyGroup())
                .WithHmqRavenDbRuntime()
                .WithRabbitMqQdActions()
                ;
        }
    }
}
