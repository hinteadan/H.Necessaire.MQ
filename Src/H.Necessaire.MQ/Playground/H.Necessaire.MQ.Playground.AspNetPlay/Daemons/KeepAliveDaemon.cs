using H.Necessaire.Runtime.Integration.NetCore;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Playground.AspNetPlay.Daemons
{
    public class KeepAliveDaemon : HostedServiceDaemonBase
    {
#if DEBUG
        static readonly TimeSpan workCycleInterval = TimeSpan.FromSeconds(5);
#else
        static readonly TimeSpan workCycleInterval = TimeSpan.FromMinutes(5);
#endif

        protected override TimeSpan WorkCycleInterval => workCycleInterval;

        string hostUrl;
        ImALogger logger;
        public override void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            base.ReferDependencies(dependencyProvider);
            logger = dependencyProvider.GetLogger<KeepAliveDaemon>();
            hostUrl = dependencyProvider.GetRuntimeConfig()?.Get("Hosting")?.Get("BaseUrl")?.ToString();
        }

        protected override async Task DoWork(CancellationToken? cancellationToken = null)
        {
            if (hostUrl.IsEmpty())
                return;

            await
                new Func<Task>(async () =>
                {
                    await logger.LogDebug($"Running keep-alive HTTP call");

                    using (new TimeMeasurement(x => logger.LogDebug($"DONE Running keep-alive HTTP call in {x}").ConfigureAwait(false).GetAwaiter().GetResult()))
                    {
                        using HttpClient httpClient = BuildNewHttpClient();
                        using HttpResponseMessage response = await httpClient.GetAsync($"{hostUrl}/ping", cancellationToken ?? CancellationToken.None);
                        await logger.LogDebug($"HTTP Keep-Alive call response: {(int)response.StatusCode} - {response.StatusCode}");
                        string content = await response.Content.ReadAsStringAsync(cancellationToken ?? CancellationToken.None);
                        await logger.LogDebug($"HTTP Keep-Alive call response content: {content}");
                    }

                })
                .TryOrFailWithGrace(onFail: async ex =>
                {
                    await logger.LogError($"Error occurred while trying to make the keep alive HTTP call. Message: {ex.Message}", ex);
                });
        }

        static HttpClient BuildNewHttpClient()
        {
            return
                new HttpClient
                (
                    handler: BuildNewStandardSocketsHttpHandler(),
                    disposeHandler: true
                );
        }

        static StandardSocketsHttpHandler BuildNewStandardSocketsHttpHandler()
        {
            return
                new StandardSocketsHttpHandler()
                {
                    // The maximum idle time for a connection in the pool. When there is no request in
                    // the provided delay, the connection is released.
                    // Default value in .NET 6: 1 minute
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),

                    // This property defines maximal connection lifetime in the pool regardless
                    // of whether the connection is idle or active. The connection is reestablished
                    // periodically to reflect the DNS or other network changes.
                    // ⚠️ Default value in .NET 6: never
                    //    Set a timeout to reflect the DNS or other network changes
                    PooledConnectionLifetime = TimeSpan.FromHours(.5),
                };
        }
    }
}
