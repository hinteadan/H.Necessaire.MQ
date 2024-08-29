using H.Necessaire.MQ.Abstractions;
using H.Necessaire.RavenDB;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.RavenDB.Concrete.Storage
{
    internal class RavenDbServiceBusStorageService
        : RavenDbStorageResourceBase<string, ServiceBusMessage, HmqEventFilter, ServiceBusMessageFilterIndex>
        , IObserver<DocumentChange>
        , IDisposable
    {
        public event EventHandler<RavenDbServiceBusMessageEventArgs> OnServiceBusMessage;
        RavenDbDocumentStore ravenDbDocumentStore;
        IDisposable ravenDbServiceBusSubscription;
        public override void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            base.ReferDependencies(dependencyProvider);

            ravenDbDocumentStore = dependencyProvider.Get<RavenDbDocumentStore>();
        }

        protected override string GetIdFor(ServiceBusMessage item) => item.ID;

        protected override async Task EnsureIndexes()
        {
            await base.EnsureIndexes();
            await EnsureIndex(() => ServiceBusMessageFilterIndex.Instance);
        }

        protected override IRavenQueryable<ServiceBusMessage> ApplyFilter(IRavenQueryable<ServiceBusMessage> query, HmqEventFilter filter)
        {
            if (filter?.IDs?.Any() == true)
                query = query.Where(x => RavenQueryableExtensions.In(x.Event.ID, filter.IDs));

            if (filter?.From != null)
                query = query.Where(x => x.Event.HappenedAt >= filter.From);

            if (filter?.To != null)
                query = query.Where(x => x.Event.HappenedAt <= filter.To);

            if (filter?.Names?.Any() == true)
                query = query.Where(x => RavenQueryableExtensions.In(x.Event.Name, filter.Names));

            if (filter?.Types?.Any() == true)
                query = query.Where(x => RavenQueryableExtensions.In(x.Event.Type, filter.Types));

            if (filter?.Assemblies?.Any() == true)
                query = query.Where(x => RavenQueryableExtensions.In(x.Event.Assembly, filter.Assemblies));

            return query;
        }


        public async Task StartListeningForServiceBusCollectionChanges()
        {
            await EnsureIndexes().ConfigureAwait(false);

            ravenDbServiceBusSubscription
                = ravenDbDocumentStore
                .Store
                .Changes(DatabaseName)
                .ForDocumentsInCollection<ServiceBusMessage>()
                .Subscribe(this)
                ;
        }

        public void Dispose()
        {
            new Action(() =>
            {
                ravenDbServiceBusSubscription?.Dispose();

            }).TryOrFailWithGrace();
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        public void OnNext(DocumentChange value)
        {
            if (value.Type != DocumentChangeTypes.Put)
                return;

            new Action(() =>
            {

                OnServiceBusMessage?.Invoke(this, new RavenDbServiceBusMessageEventArgs(value.Id));

            }).TryOrFailWithGrace();
        }
    }

    internal class ServiceBusMessageFilterIndex : AbstractIndexCreationTask<ServiceBusMessage>
    {
        public static readonly ServiceBusMessageFilterIndex Instance = new ServiceBusMessageFilterIndex();

        public ServiceBusMessageFilterIndex()
        {
            Map = docs => docs.Select(doc =>
                new
                {
                    doc.Event.ID,
                    doc.Event.HappenedAt,
                    doc.Event.Name,
                    doc.Event.Type,
                    doc.Event.Assembly,
                }
            );
        }
    }
}
