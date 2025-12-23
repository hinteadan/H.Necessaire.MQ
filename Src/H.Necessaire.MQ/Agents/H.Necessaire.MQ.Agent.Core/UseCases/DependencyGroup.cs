namespace H.Necessaire.MQ.Agent.Core.UseCases
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .RegisterAlwaysNew<ImAnHmqAgentInfoUseCase>(() => new Concrete.HmqAgentInfoUseCase())
                ;
        }
    }
}
