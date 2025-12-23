using H.Necessaire.Runtime;

namespace H.Necessaire.MQ.Agent.Core
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .Register<RuntimeDependencyGroup>(() => new RuntimeDependencyGroup())
                .Register<Concrete.DependencyGroup>(() => new Concrete.DependencyGroup())
                .Register<UseCases.DependencyGroup>(() => new UseCases.DependencyGroup())
                ;
        }
    }
}
