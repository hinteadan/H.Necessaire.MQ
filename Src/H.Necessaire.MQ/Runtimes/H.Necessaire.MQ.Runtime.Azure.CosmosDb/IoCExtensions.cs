namespace H.Necessaire.MQ.Runtime.Azure.CosmosDb
{
    public static class IoCExtensions
    {
        public static T WithHmqAzureCosmosDbRuntime<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<DependencyGroup>(() => new DependencyGroup());
            return dependencyRegistry;
        }
    }
}
