using H.Necessaire.MQ.Bus.AzureServiceBus;
using H.Necessaire.MQ.Bus.RabbitOrLavinMQ;
using H.Necessaire.MQ.CLI.Commands;
using H.Necessaire.MQ.Tools.CodeAnalysis;
using H.Necessaire.MQ.Tools.Reporting;

namespace H.Necessaire.MQ.CLI
{
    internal class DependencyGroup : ImADependencyGroup
    {
        public void RegisterDependencies(ImADependencyRegistry dependencyRegistry)
        {
            dependencyRegistry
                .WithHmqCodeAnalysisTools()
                .WithHmqReportingTools()
                .WithRabbitMqQdActions()
                //.WithAzureServiceBusQdActions()
                .Register<DebugCommand.DevTestQdActionProcessor>(() => new DebugCommand.DevTestQdActionProcessor())
                //.StartRabbitMqQdActionsProcessor()
                //.StartAzureServiceBusQdActionsProcessor()
                ;
        }
    }
}
