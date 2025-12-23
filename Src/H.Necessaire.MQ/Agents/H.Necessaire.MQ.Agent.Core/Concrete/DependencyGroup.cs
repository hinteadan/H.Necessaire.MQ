using H.Necessaire.MQ.Agent.Abstractions;

namespace H.Necessaire.MQ.Agent.Core.Concrete
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .Register<ImAnHmqAgentInfoProvider>(() => new HmqAgentInfoProvider())
                ;
        }
    }
}
