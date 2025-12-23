using H.Necessaire.MQ.Agent.Host.AspNetCore;
using H.Necessaire.Runtime.Integration.AspNetCore;

namespace H.Necessaire.MQ.Agent.Sample.AspNetCore
{
    public class Program
    {
        static readonly ImADependencyRegistry dependencyRegistry
             = IoC
            .NewDependencyRegistry()
            .WithDefaultHAppConfig()
            .Register<DependencyGroup>(() => new DependencyGroup())
            ;

        public static async Task Main(string[] args)
        {
            var builder
                = WebApplication
                .CreateBuilder(args)
                .WithHmqAspNetHostAgent(dependencyRegistry)
                ;

            var app = builder.Build().BindToHNecessaireAspNetRuntime(dependencyRegistry);

            app.ConfigureHmqAspNetHostAgent(app.Environment);

            if (!app.Environment.IsEnvironment("Local"))
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            //app.MapGet("/", () => "Hello World!");

            await dependencyRegistry.StartHmqAspNetHostAgent();

            await app.RunAsync();
        }
    }
}
