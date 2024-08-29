namespace H.Necessaire.MQ.Bus.FileSystem.Concrete.Storage
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry

                .Register<ServiceBusJsonCachedFileSystemStorageService>(() => new ServiceBusJsonCachedFileSystemStorageService())

                ;
        }
    }
}
