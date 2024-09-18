namespace H.Necessaire.MQ.Bus.Commons
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .Register<ResilienceRecoveryDaemon>(() => new ResilienceRecoveryDaemon())
                ;
        }
    }
}
