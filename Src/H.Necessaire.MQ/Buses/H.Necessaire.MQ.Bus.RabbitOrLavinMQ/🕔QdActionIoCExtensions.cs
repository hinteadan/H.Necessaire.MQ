using H.Necessaire.MQ.Bus.Commons;
using H.Necessaire.MQ.Bus.RabbitOrLavinMQ.Concrete.QdActions;

namespace H.Necessaire.MQ.Bus.RabbitOrLavinMQ
{
    public static class QdActionIoCExtensions
    {
        public static T WithRabbitMqQdActions<T>(this T dependencyRegistry) where T : ImADependencyRegistry
        {
            dependencyRegistry.Register<Concrete.QdActions.DependencyGroup>(() => new Concrete.QdActions.DependencyGroup());
            return dependencyRegistry;
        }

        public static T StartRabbitMqQdActionsProcessor<T>(this T dependencyProvider) where T : ImADependencyProvider
        {
            ImADaemon rabbitMqQdActionsProcessingDaemon = dependencyProvider.Get<RabbitMqQdActionProcessingDaemon>();

            rabbitMqQdActionsProcessingDaemon
                .Start()
                .ConfigureAwait(continueOnCapturedContext: false)
                .GetAwaiter()
                .GetResult()
                ;

            dependencyProvider.Get<ResilienceRecoveryDaemon>().Start();

            return dependencyProvider;
        }

        public static T StopRabbitMqQdActionsProcessor<T>(this T dependencyProvider) where T : ImADependencyProvider
        {
            ImADaemon rabbitMqQdActionsProcessingDaemon = dependencyProvider.Get<RabbitMqQdActionProcessingDaemon>();

            rabbitMqQdActionsProcessingDaemon
                .Stop()
                .ConfigureAwait(continueOnCapturedContext: false)
                .GetAwaiter()
                .GetResult()
                ;

            return dependencyProvider;
        }
    }
}
