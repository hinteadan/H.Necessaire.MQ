using H.Necessaire.MQ.Bus.AzureServiceBus;
using H.Necessaire.MQ.Bus.RabbitOrLavinMQ;
using H.Necessaire.MQ.CLI.Commands;

namespace H.Necessaire.MQ.CLI
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .WithRabbitMqQdActions()
                //.WithAzureServiceBusQdActions()
                .Register<DebugCommand.DevTestQdActionProcessor>(() => new DebugCommand.DevTestQdActionProcessor())
                .StartRabbitMqQdActionsProcessor()
                //.StartAzureServiceBusQdActionsProcessor()
                ;
        }
    }
}
