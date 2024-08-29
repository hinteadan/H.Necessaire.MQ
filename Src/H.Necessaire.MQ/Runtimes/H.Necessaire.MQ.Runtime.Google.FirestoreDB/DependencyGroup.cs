namespace H.Necessaire.MQ.Runtime.Google.FirestoreDB
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
