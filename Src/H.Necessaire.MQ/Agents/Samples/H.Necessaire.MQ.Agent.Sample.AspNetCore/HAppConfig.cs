using System.Reflection;

namespace H.Necessaire.MQ.Agent.Sample.AspNetCore
{
    internal static class HAppConfig
    {
#if DEBUG
        public const bool IsDebug = true;
#else
        public const bool IsDebug = false;
#endif

        static readonly Assembly mainAssembly = typeof(HAppConfig).Assembly;
        static readonly RuntimeConfig defaultRuntimeConfig = new()
        {
            Values = [
                "App".ConfigWith(
                    "Name".ConfigWith(mainAssembly.GetName().Name)
                ),
                "IronMen".ConfigWith(
                    "FilePath".ConfigWith(Path.Combine("Secrets", "ironmen.json")),
                    "PassFilePath".ConfigWith(Path.Combine("Secrets", "ironmen.pass.json"))
                ),
                "QdActions".ConfigWith(
                    "RabbitMQ".ConfigWith(
                        "URL".ConfigWith("RabbitMq.URL.cfg.txt".ReadSecretFromEmbeddedResources(typeof(HAppConfig).Assembly))
                    )
                ),
                "RavenDbConnections".ConfigWith(
                    "ClientCertificateName".ConfigWith("RoVFR.Showcase.RavenDb.cert.pfx")
                    , "ClientCertificatePassword".ConfigWithSecretFromEmbeddedResources("RoVFR.Showcase.RavenDb.cert.pass.txt", mainAssembly)
                    , "DatabaseUrls".ConfigWith(
                        "DefaultUrl".ConfigWithSecretFromEmbeddedResources("RoVFR.Showcase.RavenDb.url.txt", mainAssembly)
                    )
                    , "DatabaseNames".ConfigWith(
                        "Core".ConfigWith($"HMQ_Agents_Sample_Core{(IsDebug ? "_DEBUG" : "")}")
                        , "Default".ConfigWith($"HMQ_Agents_Sample{(IsDebug ? "_DEBUG" : "")}")
                    )
                ),
            ],
        };

        public static T WithDefaultHAppConfig<T>(this T depsRegistry) where T : ImADependencyRegistry
        {
            depsRegistry.Register<RuntimeConfig>(() => defaultRuntimeConfig);
            return depsRegistry;
        }
    }
}
