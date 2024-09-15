using H.Necessaire.Runtime.CLI.Commands;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.CLI.Commands
{
    [Alias("dbg")]
    internal class DebugCommand : CommandBase
    {
        ImAnActionQer actionQer;
        ImAStorageBrowserService<QdAction, QdActionFilter> queueBrowser;
        ImAQdActionQueueOnDemandRunner queueOnDemandRunner;
        public override void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            base.ReferDependencies(dependencyProvider);

            actionQer = dependencyProvider.Get<ImAnActionQer>();
            queueBrowser = dependencyProvider.Get<ImAStorageBrowserService<QdAction, QdActionFilter>>();
            queueOnDemandRunner = dependencyProvider.Get<ImAQdActionQueueOnDemandRunner>();
        }

        public override async Task<OperationResult> Run()
        {
            await Task.CompletedTask;

            Log($"Debugging");
            using (new TimeMeasurement(x => Log($"DONE Debugging in  {x}")))
            {
                await Task.WhenAll([
                    actionQer.Queue(QdAction.New("DevTest", "Test 1")),
                    actionQer.Queue(QdAction.New("DevTest", "Test 2")),
                    actionQer.Queue(QdAction.New("DevTest", "Test 3")),
                    actionQer.Queue(QdAction.New("DevTest", "Test 4")),
                    actionQer.Queue(QdAction.New("DevTest", "Test 5")),
                ]);
            }

            await Task.Delay(3500);

            return OperationResult.Win();
        }



        internal class DevTestQdActionProcessor : QdActionProcessorBase
        {
            static readonly string[] supportedQdActionTypes = ["DevTest"];
            protected override string[] SupportedQdActionTypes => supportedQdActionTypes;

            protected override async Task<OperationResult> ProcessQdAction(QdAction action)
            {
                await Task.CompletedTask;

                return OperationResult.Win();
            }
        }
    }
}
