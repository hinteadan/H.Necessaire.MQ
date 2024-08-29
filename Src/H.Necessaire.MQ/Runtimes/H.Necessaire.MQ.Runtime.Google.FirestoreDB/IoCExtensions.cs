namespace H.Necessaire.MQ.Runtime.Google.FirestoreDB
{
    public static class IoCExtensions
    {
        public static T WithHmqGoogleFirestoreDbRuntime<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<DependencyGroup>(() => new DependencyGroup());
            return dependencyRegistry;
        }
    }
}
