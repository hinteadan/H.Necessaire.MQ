using H.Necessaire.CLI;
using H.Necessaire.Runtime.CLI;

namespace H.Necessaire.MQ.CLI
{
    internal class Program
    {
        public static void Main()
        {
            new CliApp()
                .WithEverything()
                .WithDefaultRuntimeConfig()
                .With(x => x.Register<DependencyGroup>(() => new DependencyGroup()))
                .Run(askForCommandIfEmpty: true)
                .GetAwaiter()
                .GetResult()
                ;
        }
    }
}
