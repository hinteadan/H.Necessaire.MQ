namespace H.Necessaire.MQ.Playground.AspNetPlay.Daemons
{
    public class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .Register<KeepAliveDaemon>(() => new KeepAliveDaemon())
                ;
        }
    }
}
