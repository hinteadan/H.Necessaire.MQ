namespace H.Necessaire.MQ.Playground.AspNetPlay
{
    public class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry

                .Register<Security.DependencyGroup>(() => new Security.DependencyGroup())

                .Register<Daemons.DependencyGroup>(() => new Daemons.DependencyGroup())

                .Register<UseCases.DependencyGroup>(() => new UseCases.DependencyGroup())

                ;
        }
    }
}
