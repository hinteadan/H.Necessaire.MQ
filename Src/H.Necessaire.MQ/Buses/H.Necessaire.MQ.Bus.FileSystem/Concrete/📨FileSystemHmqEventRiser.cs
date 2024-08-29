using H.Necessaire.MQ.Abstractions;
using H.Necessaire.MQ.Bus.FileSystem.Concrete.Storage;
using H.Necessaire.MQ.Core;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.FileSystem.Concrete
{
    internal class FileSystemHmqEventRiser : ImAnHmqEventRiser, ImADependency
    {
        ServiceBusJsonCachedFileSystemStorageService hmqEventsFileSystem;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            hmqEventsFileSystem = dependencyProvider.Get<ServiceBusJsonCachedFileSystemStorageService>();
        }

        public async Task<OperationResult<ImAnHmqReActor>[]> Raise(HmqEvent hmqEvent)
        {
            HmqEvent eventToSend = hmqEvent.Clone().MarkAsPersisted();

            OperationResult result = await hmqEventsFileSystem.Save(eventToSend);

            return result.WithPayload(FileSystemReActor.Instance).AsArray();
        }
    }
}
