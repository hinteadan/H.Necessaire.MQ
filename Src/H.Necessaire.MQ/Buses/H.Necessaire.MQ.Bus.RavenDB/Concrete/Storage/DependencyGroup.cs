namespace H.Necessaire.MQ.Bus.RavenDB.Concrete.Storage
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry

                .Register<RavenDbServiceBusStorageService>(() => new RavenDbServiceBusStorageService())

                ;
        }
    }
}
