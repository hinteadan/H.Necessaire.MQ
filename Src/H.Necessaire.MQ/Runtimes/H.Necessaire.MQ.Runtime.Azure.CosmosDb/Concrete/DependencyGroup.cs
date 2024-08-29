namespace H.Necessaire.MQ.Runtime.Azure.CosmosDb.Concrete
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .Register<Storage.DependencyGroup>(() => new Storage.DependencyGroup())
                ;
        }
    }
}
