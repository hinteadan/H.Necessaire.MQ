using System;
using System.IO;

namespace H.Necessaire.MQ.Bus.FileSystem.Concrete.Storage
{
    internal class FileSystemTriggerHmqEventArgs : EventArgs
    {
        public FileSystemTriggerHmqEventArgs(FileInfo eventFile)
        {
            EventFile = eventFile;
        }

        public FileInfo EventFile { get; }
    }
}
