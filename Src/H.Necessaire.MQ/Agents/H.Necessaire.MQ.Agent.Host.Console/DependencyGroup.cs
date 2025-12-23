using H.Necessaire.MQ.Agent.Core;

namespace H.Necessaire.MQ.Agent.Host.Console
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .WithHmqAgentCore()
                .RegisterAlwaysNew<HmqAgentCommand>(() => new HmqAgentCommand())
                ;
        }
    }
}
