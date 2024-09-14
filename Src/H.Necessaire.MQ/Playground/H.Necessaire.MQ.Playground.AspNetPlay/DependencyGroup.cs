using H.Necessaire.MQ.Bus.RabbitOrLavinMQ;
using H.Necessaire.Runtime.Integration.NetCore;
using System;
using System.Linq;

namespace H.Necessaire.MQ.Playground.AspNetPlay
{
    public class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            Type qdActionProcessingHostedServiceDaemonType
                = typeof(ImADaemon)
                .GetAllImplementations(typeof(NetCoreLoggingExtensions).Assembly)
                .Single(x => x.Name == "QdActionProcessingHostedServiceDaemon")
                ;

            dependencyRegistry

                .Unregister(qdActionProcessingHostedServiceDaemonType)

                .Register<Runtime.SqlServer.SqlServerRuntimeDependencyGroup>(() => new Runtime.SqlServer.SqlServerRuntimeDependencyGroup())

                .Register<Security.DependencyGroup>(() => new Security.DependencyGroup())

                .Register<Daemons.DependencyGroup>(() => new Daemons.DependencyGroup())

                .Register<UseCases.DependencyGroup>(() => new UseCases.DependencyGroup())

                //.WithHmq()

                //.WithHmqRabbitMqMessageBus()

                .WithRabbitMqQdActions()

                //.StartHmqRabbitMqExternalListener()
                .StartRabbitMqQdActionsProcessor()

                ;
        }
    }
}
