namespace H.Necessaire.MQ.Runtime.RavenDb
{
    public static class IoCExtensions
    {
        public static T WithHmqRavenDbRuntime<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<DependencyGroup>(() => new DependencyGroup());
            return dependencyRegistry;
        }
    }
}
