using H.Necessaire.CLI.Commands;
using H.Necessaire.Runtime.ExternalCommandRunner;
using System;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.CLI.Commands
{
    [Alias("dbg")]
    internal class DebugCommand : CommandBase
    {
        const int numberOfMessagesToPublish = 50;
        ImAnActionQer actionQer;
        ImAStorageBrowserService<QdAction, QdActionFilter> queueBrowser;
        ImAQdActionQueueOnDemandRunner queueOnDemandRunner;
        ExternalCommandRunner externalCommandRunner;
        public override void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            base.ReferDependencies(dependencyProvider);

            actionQer = dependencyProvider.Get<ImAnActionQer>();
            queueBrowser = dependencyProvider.Get<ImAStorageBrowserService<QdAction, QdActionFilter>>();
            queueOnDemandRunner = dependencyProvider.Get<ImAQdActionQueueOnDemandRunner>();

            externalCommandRunner = dependencyProvider.Get<ExternalCommandRunner>();
        }

        public override async Task<OperationResult> Run()
        {
            OperationResult<ExternalCommandRunContext> result = await externalCommandRunner
                .WithContext(new ExternalCommandRunContext
                {
                    IsOutputCaptured = true,
                    IsUserInputExpected = true,
                    UserInputProvider = () => new string[] {
                        "ping google.com",
                        "ping hintea.com",
                        "exit",
                    }.AsTask(),
                })
                .RunCmd()
                ;

            string output = result.Payload.OutputData.ToString();

            return result;

            await Task.CompletedTask;

            Log($"Debugging");
            using (new TimeMeasurement(x => Log($"DONE Debugging in  {x}")))
            {
                for (int index = 0; index < numberOfMessagesToPublish; index++)
                {
                    await actionQer.Queue(QdAction.New("DevTest", $"Test {index + 1}"));
                }
            }

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();

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
