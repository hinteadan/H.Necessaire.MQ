using H.Necessaire.Runtime;
using H.Necessaire.Runtime.Integration.NetCore;
using System;
using System.Linq;

namespace H.Necessaire.MQ.Playground.AspNetPlay
{
    public class AppWireup : NetCoreApiWireupBase
    {
        public override ImAnApiWireup WithEverything()
        {
            Type qdActionProcessingHostedServiceDaemonType 
                = typeof(ImADaemon)
                .GetAllImplementations(typeof(NetCoreLoggingExtensions).Assembly)
                .Single(x => x.Name == "QdActionProcessingHostedServiceDaemon")
                ;

            return
                base
                .WithEverything()
                .With(x => x.Unregister(qdActionProcessingHostedServiceDaemonType))
                .With(x => x.Register<DependencyGroup>(() => new DependencyGroup()))
                .With(x => x.Register<Runtime.SqlServer.SqlServerRuntimeDependencyGroup>(() => new Runtime.SqlServer.SqlServerRuntimeDependencyGroup()))
                ;
        }
    }
}
