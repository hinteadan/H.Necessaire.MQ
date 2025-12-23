using H.Necessaire.MQ.Agent.Host.Console;
using H.Necessaire.MQ.Bus.RabbitOrLavinMQ;
using H.Necessaire.MQ.Runtime.RavenDb;
using H.Necessaire.Operations.Versioning.Concrete;
using H.Necessaire.Runtime.RavenDB;

namespace H.Necessaire.MQ.Agent.Sample.Console
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .Register<ImAVersionProvider>(() => new EmbeddedResourceVersionProvider(typeof(DependencyGroup).Assembly))
                .WithHmqConsoleHostAgent()
                .Register<RavenDbRuntimeDependencyGroup>(() => new RavenDbRuntimeDependencyGroup())
                .WithHmqRavenDbRuntime()
                //.WithHmqRabbitMqMessageBus()
                .WithRabbitMqQdActions()
                ;
        }
    }
}
