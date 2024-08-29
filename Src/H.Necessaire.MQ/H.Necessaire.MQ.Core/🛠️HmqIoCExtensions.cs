using H.Necessaire.MQ.Abstractions;

namespace H.Necessaire.MQ.Core
{
    public static class HmqIoCExtensions
    {
        public static T StartHmqExternalListener<T>(this T dependencyProvider, string buildTypeID) where T : ImADependencyProvider
        {
            ImAnHmqExternalEventListener externalListener
                = dependencyProvider.Build<ImAnHmqExternalEventListener>(buildTypeID);
            externalListener.Start().ConfigureAwait(false).GetAwaiter().GetResult();
            return dependencyProvider;
        }
    }
}
