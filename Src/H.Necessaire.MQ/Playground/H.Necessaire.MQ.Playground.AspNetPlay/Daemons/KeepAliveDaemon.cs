using H.Necessaire.Runtime.Integration.NetCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Playground.AspNetPlay.Daemons
{
    public class KeepAliveDaemon : HostedServiceDaemonBase
    {
        static readonly EphemeralType<HttpClient> ephemeralHttpClient
            = new EphemeralType<HttpClient> { ValidFor = TimeSpan.FromMinutes(15), ValidFrom = DateTime.MinValue };
#if DEBUG
        static readonly TimeSpan workCycleInterval = TimeSpan.FromSeconds(5);
#else
        static readonly TimeSpan workCycleInterval = TimeSpan.FromMinutes(5);
#endif

        protected override TimeSpan WorkCycleInterval => workCycleInterval;

        string[] hostUrls;
        ImALogger logger;
        public override void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            base.ReferDependencies(dependencyProvider);
            logger = dependencyProvider.GetLogger<KeepAliveDaemon>();
            hostUrls = ParseUrls(dependencyProvider.GetRuntimeConfig()?.Get("ASPNETCORE_URLS")?.ToString());
        }

        protected override async Task DoWork(CancellationToken? cancellationToken = null)
        {
            if (hostUrls?.Any() != true)
                return;

            await
                Task.WhenAll(hostUrls.Select(x => PingUrl(x, cancellationToken)));
        }

        async Task PingUrl(string hostUrl, CancellationToken? cancellationToken = null)
        {
            await
                new Func<Task>(async () =>
                {
                    await logger.LogDebug($"Running keep-alive HTTP call");

                    using (new TimeMeasurement(x => logger.LogDebug($"DONE Running keep-alive HTTP call in {x}").ConfigureAwait(false).GetAwaiter().GetResult()))
                    {
                        HttpClient httpClient = GetHttpClient();
                        using HttpResponseMessage response = await httpClient.GetAsync($"{hostUrl}health", cancellationToken ?? CancellationToken.None);
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

        static HttpClient GetHttpClient()
        {
            if (ephemeralHttpClient.IsActive() && ephemeralHttpClient.Payload is not null)
                return ephemeralHttpClient.Payload;

            if (ephemeralHttpClient.IsExpired() && ephemeralHttpClient.Payload is not null)
                ephemeralHttpClient.Payload.Dispose();

            ephemeralHttpClient.Payload
                = new HttpClient
                (
                    handler: BuildNewStandardSocketsHttpHandler(),
                    disposeHandler: true
                );
            ephemeralHttpClient.ActiveAsOf(DateTime.UtcNow);

            return ephemeralHttpClient.Payload;
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

        static string[] ParseUrls(string urlsAsString)
        {
            if (urlsAsString.IsEmpty())
                return null;

            string[] parts = urlsAsString.Split(';', StringSplitOptions.RemoveEmptyEntries);

            return parts?.Select(ParseUrl)?.ToNoNullsArray();
        }

        static string ParseUrl(string urlAsString)
        {
            if (urlAsString.IsEmpty())
                return null;

            if(!Uri.TryCreate(urlAsString, UriKind.Absolute, out Uri parsedUri))
                return null;

            return parsedUri.ToString();
        }
    }
}
