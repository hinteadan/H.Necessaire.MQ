using H.Necessaire.CLI.Commands;
using H.Necessaire.Runtime;

namespace H.Necessaire.MQ.Agent.Sample.Console
{
    internal class DebugCommand : CommandBase
    {
        public override Task<OperationResult> Run() => RunSubCommand();

        class DebugSubCommand : SubCommandBase
        {
            ImAnActionQer actionQer;
            public override void ReferDependencies(ImADependencyProvider dependencyProvider)
            {
                base.ReferDependencies(dependencyProvider);
                actionQer = dependencyProvider.Get<ImAnActionQer>();
            }
            public override async Task<OperationResult> Run(params Note[] args)
            {
                OperationResult res = await actionQer.Queue(QdAction.New(WellKnown.QdActionType.ProcessIpAddress, WellKnown.QdActionType.ProcessIpAddressPayload("89.136.54.52", null)));

                Log("Debugging. Press Ctrl+C or Ctrl+Break to stop.");

                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancelToken = cancellationTokenSource.Token;

                System.Console.CancelKeyPress += (s, e) => { e.Cancel = true; cancellationTokenSource.Cancel(); };

                await HSafe.Run(async () => await Task.Delay(Timeout.InfiniteTimeSpan, cancelToken));

                return true;
            }
        }
    }
}
