using H.Necessaire.RavenDB;

namespace H.Necessaire.MQ.Bus.RavenDB
{
    internal class DependencyGroup : RavenDbDependencyGroup
    {
        public override void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            base.RegisterDependencies(dependencyRegistry);

            dependencyRegistry

                .Register<Concrete.DependencyGroup>(() => new Concrete.DependencyGroup())

                ;
        }
    }
}