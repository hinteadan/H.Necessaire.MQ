using H.Necessaire.Runtime;
using H.Necessaire.Runtime.Integration.NetCore;

namespace H.Necessaire.MQ.Playground.AspNetPlay
{
    public class AppWireup : NetCoreApiWireupBase
    {
        public override ImAnApiWireup WithEverything()
        {
            return
                base
                .WithEverything()
                .With(x => x.Register<DependencyGroup>(() => new DependencyGroup()))
                .With(x => x.Register<Runtime.SqlServer.SqlServerRuntimeDependencyGroup>(() => new Runtime.SqlServer.SqlServerRuntimeDependencyGroup()))
                ;
        }
    }
}
