using H.Necessaire.CLI;
using H.Necessaire.Runtime.CLI;

namespace H.Necessaire.MQ.Agent.Sample.Console
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await
                new CliApp()
                .WithEverything()
                .With(x => x.WithDefaultHAppConfig().Register<DependencyGroup>(() => new DependencyGroup()))
                .Run()
                ;
        }
    }
}
