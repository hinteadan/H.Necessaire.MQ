using H.Necessaire.MQ.Agent.Core;
using H.Necessaire.Runtime;
using H.Necessaire.Runtime.Integration.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace H.Necessaire.MQ.Agent.Host.AspNetCore
{
    public static class IoCXtnx
    {
        public static T WithHmqAspNetHostAgent<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<DependencyGroup>(() => new DependencyGroup());
            return dependencyRegistry;
        }

        public static async Task StartHmqAspNetHostAgent<T>(this T dependencyProvider) where T : ImADependencyProvider
        {
            if (dependencyProvider is ImADependencyRegistry dependencyRegistry)
            {
                Type[] allQdActionsDaemons = typeof(ImADaemon).GetAllImplementations().Where(t => t.Name.Contains("QdAction", StringComparison.InvariantCultureIgnoreCase)).ToArrayNullIfEmpty();
                Type[] hQdActionsDaemons = allQdActionsDaemons?.Where(t => t.Name.In("QdActionProcessingHostedServiceDaemon", "QdActionProcessingDaemon")).ToArray();
                Type[] extraQdActionsDaemons = allQdActionsDaemons?.Except(hQdActionsDaemons ?? []).ToArray();

                if (!extraQdActionsDaemons.IsEmpty())
                {
                    foreach (Type hQdActionsDaemon in hQdActionsDaemons)
                    {
                        dependencyRegistry.Unregister(hQdActionsDaemon);
                    }
                }
            }

            await dependencyProvider.StartHmqAgent();
        }

        public static THostApplicationBuilder WithHmqAspNetHostAgent<THostApplicationBuilder>(this THostApplicationBuilder hostApplicationBuilder, ImADependencyRegistry dependencyRegistry)
            where THostApplicationBuilder : IHostApplicationBuilder
        {
            hostApplicationBuilder.WithHNecessaire(dependencyRegistry);

            dependencyRegistry.Unregister<SyncRequestProcessingDaemon>();

            hostApplicationBuilder.Services.AddControllers().ConfigureApplicationPartManager(apm => {
                apm.ApplicationParts.Add(new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(typeof(IoCXtnx).Assembly));
            }).AddControllersAsServices();

            return hostApplicationBuilder;
        }

        public static TApplicationBuilder ConfigureHmqAspNetHostAgent<TApplicationBuilder>(this TApplicationBuilder applicationBuilder, IHostEnvironment hostEnvironment)
            where TApplicationBuilder : IApplicationBuilder
        {
            applicationBuilder.ConfigureHNecessaireAspNetRuntime(hostEnvironment);

            applicationBuilder.UseEndpoints(x => x.MapControllers());

            return applicationBuilder;
        }
    }
}
