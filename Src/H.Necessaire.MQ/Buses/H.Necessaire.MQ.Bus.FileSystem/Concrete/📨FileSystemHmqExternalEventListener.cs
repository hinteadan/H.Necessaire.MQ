using H.Necessaire.MQ.Abstractions;
using H.Necessaire.MQ.Bus.FileSystem.Concrete.Storage;
using H.Necessaire.Serialization;
using System;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.FileSystem.Concrete
{
    [ID("FileSystem")]
    [Alias("fs")]
    internal class FileSystemHmqExternalEventListener : ImAnHmqExternalEventListener, ImADependency, IDisposable
    {
        ServiceBusJsonCachedFileSystemStorageService hmqEventsFileSystem;
        ImAnHmqEventRiser internalEventRiser;
        ImALogger logger;
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            hmqEventsFileSystem = dependencyProvider.Get<ServiceBusJsonCachedFileSystemStorageService>();
            internalEventRiser = dependencyProvider.Build<ImAnHmqEventRiser>("internal");
            logger = dependencyProvider.GetLogger<FileSystemHmqExternalEventListener>();
        }

        public Task<OperationResult> Start()
        {
            hmqEventsFileSystem.OnFileSystemTriggerEvent += HmqEventsFileSystem_OnFileSystemTriggerEvent;
            return OperationResult.Win().AsTask();
        }

        public Task<OperationResult> Stop()
        {
            hmqEventsFileSystem.OnFileSystemTriggerEvent -= HmqEventsFileSystem_OnFileSystemTriggerEvent;
            hmqEventsFileSystem.Dispose();
            return OperationResult.Win().AsTask();
        }

        public void Dispose()
        {
            new Action(() =>
            {
                Stop().ConfigureAwait(false).GetAwaiter().GetResult();

            }).TryOrFailWithGrace();
        }

        private async void HmqEventsFileSystem_OnFileSystemTriggerEvent(object sender, FileSystemTriggerHmqEventArgs e)
        {
            if (e?.EventFile?.Exists != true)
            {
                await logger.LogError($"Error occured while handling FileSystem event for {e?.EventFile?.FullName}. The given event file doesn't exist.");
                return;
            }

            await
                new Func<Task>(async () =>
                {

                    string serializedEventReceived = await e.EventFile.OpenRead().ReadAsStringAsync(isStreamLeftOpen: false);
                    ServiceBusMessage serviceBusMessage = serializedEventReceived.TryJsonToObject<ServiceBusMessage>().ThrowOnFailOrReturn();
                    HmqEvent hmqEvent = serviceBusMessage.Event;
                    await internalEventRiser.Raise(hmqEvent);

                })
                .TryOrFailWithGrace(onFail: async ex =>
                {
                    await logger.LogError($"Error occured while handling FileSystem event for {e.EventFile.FullName}. Message: {ex.Message}", ex);
                });
        }
    }
}
