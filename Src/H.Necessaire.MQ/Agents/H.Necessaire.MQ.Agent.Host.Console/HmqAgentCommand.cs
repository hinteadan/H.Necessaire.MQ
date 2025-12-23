using H.Necessaire.CLI.Commands;

namespace H.Necessaire.MQ.Agent.Host.Console
{
    [Alias("hmq", "agent")]
    public class HmqAgentCommand : CommandBase
    {
        ImADependencyProvider dependencyProvider;
        public override void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            base.ReferDependencies(dependencyProvider);
            this.dependencyProvider = dependencyProvider;
        }

        public override async Task<OperationResult> Run()
        {
            await dependencyProvider.StartHmqConsoleHostAgent();

            Log("HMQ Console Host Agent started. Press Ctrl+C or Ctrl+Break to stop.");

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = cancellationTokenSource.Token;

            System.Console.CancelKeyPress += (s, e) => { e.Cancel = true; cancellationTokenSource.Cancel(); };

            await HSafe.Run(async () => await Task.Delay(Timeout.InfiniteTimeSpan, cancelToken));

            return true;
        }
    }
}
