using H.Necessaire.MQ.Abstractions;
using H.Necessaire.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.FileSystem.Concrete.Storage
{
    [ID("FileSystemMessageBus")]
    internal class ServiceBusJsonCachedFileSystemStorageService : CachedFileSystemStorageServiceBase<Guid, ServiceBusMessage, HmqEventFilter>, IDisposable
    {
        static readonly TimeSpan maxWaitForFileWriteCompletion = TimeSpan.FromSeconds(30);
        static readonly TimeSpan fileWriteCompletionCheckFrequency = TimeSpan.FromSeconds(.25);
        public event EventHandler<FileSystemTriggerHmqEventArgs> OnFileSystemTriggerEvent;
        readonly FileSystemWatcher messageBusFolderWatcher;
        public ServiceBusJsonCachedFileSystemStorageService()
            : base(rootFolder: GetFileSystemMessageBusFolderFromStartAssembly(), fileExtension: "bus.event.json")
        {
            entityStorageFolder = GetFileSystemMessageBusFolderFromStartAssembly();
            EnsureEntityStorageFolder().ConfigureAwait(false).GetAwaiter().GetResult();
            messageBusFolderWatcher = new FileSystemWatcher(entityStorageFolder.FullName, "*.bus.event.json");
            messageBusFolderWatcher.EnableRaisingEvents = true;
            messageBusFolderWatcher.Created += MessageBusFolderWatcher_Created;
        }

        private async void MessageBusFolderWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created)
                return;

            FileInfo file = new FileInfo(e.FullPath);

            await WaitForFileWriteCompletion(file);

            new Action(() =>
            {

                OnFileSystemTriggerEvent?.Invoke(this, new FileSystemTriggerHmqEventArgs(file));

            }).TryOrFailWithGrace();
        }

        async Task WaitForFileWriteCompletion(FileInfo fileInfo)
        {
            if (fileInfo?.Exists != true)
                return;

            Stopwatch stopwatch = Stopwatch.StartNew();

            while (!CanReadFile(fileInfo))
            {
                await Task.Delay(fileWriteCompletionCheckFrequency);

                if (stopwatch.Elapsed >= maxWaitForFileWriteCompletion)
                    break;
            }
        }

        bool CanReadFile(FileInfo file)
        {
            try
            {
                using (file.Open(FileMode.Open, FileAccess.Read, FileShare.None)) { }
            }
            catch (IOException)
            {
                return false;
            }

            return true;
        }

        protected override IEnumerable<ServiceBusMessage> ApplyFilter(IEnumerable<ServiceBusMessage> stream, HmqEventFilter filter)
        {
            if (filter?.IDs?.Any() == true)
                stream = stream.Where(x => x.Event.ID.In(filter.IDs));

            if (filter?.From != null)
                stream = stream.Where(x => x.Event.HappenedAt >= filter.From);

            if (filter?.To != null)
                stream = stream.Where(x => x.Event.HappenedAt <= filter.To);

            if (filter?.Names?.Any() == true)
                stream = stream.Where(x => x.Event.Name.In(filter.Names, (item, key) => item.Is(key)));

            if (filter?.Types?.Any() == true)
                stream = stream.Where(x => x.Event.Type.In(filter.Types, (item, key) => item.Is(key)));

            if (filter?.Assemblies?.Any() == true)
                stream = stream.Where(x => x.Event.Assembly.In(filter.Assemblies, (item, key) => item.Is(key)));

            return stream;
        }

        protected override async Task<ServiceBusMessage> ReadAndParseEntityFromStream(Stream serializedEntityStream)
        {
            return
                (await serializedEntityStream.ReadAsStringAsync())
                .TryJsonToObject<ServiceBusMessage>()
                .ThrowOnFailOrReturn()
                ;
        }

        protected override async Task SerializeEntityToStream(ServiceBusMessage entityToSerialize, Stream entitySerializationStream)
        {
            await
                entityToSerialize
                .ToJsonObject(isPrettyPrinted: true)
                .WriteToStreamAsync(entitySerializationStream)
                ;
        }

        private static DirectoryInfo GetFileSystemMessageBusFolderFromStartAssembly()
        {
            return new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetEntryAssembly().CodeBase).Path)), "FileSystemMessageBus"));
        }

        public void Dispose()
        {
            new Action(() =>
            {
                messageBusFolderWatcher.Created -= MessageBusFolderWatcher_Created;
                messageBusFolderWatcher.Dispose();

            }).TryOrFailWithGrace();
        }
    }
}
