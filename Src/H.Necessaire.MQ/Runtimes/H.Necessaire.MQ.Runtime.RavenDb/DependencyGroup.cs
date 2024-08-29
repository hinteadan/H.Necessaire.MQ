namespace H.Necessaire.MQ.Runtime.RavenDb
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .Register<Concrete.DependencyGroup>(() => new Concrete.DependencyGroup())
                ;
        }
    }
}
