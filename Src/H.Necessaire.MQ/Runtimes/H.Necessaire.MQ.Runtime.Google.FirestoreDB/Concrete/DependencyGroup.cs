namespace H.Necessaire.MQ.Runtime.Google.FirestoreDB.Concrete
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
