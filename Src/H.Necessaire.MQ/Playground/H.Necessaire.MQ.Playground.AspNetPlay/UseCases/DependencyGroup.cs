namespace H.Necessaire.MQ.Playground.AspNetPlay.UseCases
{
    public class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .RegisterAlwaysNew<QdActionUseCase>(() => new QdActionUseCase())
                ;
        }
    }
}
