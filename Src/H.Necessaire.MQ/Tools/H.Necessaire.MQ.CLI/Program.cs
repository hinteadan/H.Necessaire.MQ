using H.Necessaire.CLI;
using H.Necessaire.Runtime.CLI;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.CLI
{
    internal class Program
    {
        public static async Task Main()
        {
            await
                new CliApp()
                .WithEverything()
                .WithDefaultRuntimeConfig()
                .With(x => x.Register<DependencyGroup>(() => new DependencyGroup()))
                .Run()
                ;
        }
    }
}
