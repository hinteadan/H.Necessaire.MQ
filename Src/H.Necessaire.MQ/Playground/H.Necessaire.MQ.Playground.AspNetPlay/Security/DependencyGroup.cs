using H.Necessaire.Runtime;

namespace H.Necessaire.MQ.Playground.AspNetPlay.Security
{
    public class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .Register<ImTheIronManProviderResource>(() => new EnvironmentVariablesIronMenProviderResource())
                ;
        }
    }
}
