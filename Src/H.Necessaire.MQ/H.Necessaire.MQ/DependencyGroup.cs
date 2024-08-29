namespace H.Necessaire.MQ
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
