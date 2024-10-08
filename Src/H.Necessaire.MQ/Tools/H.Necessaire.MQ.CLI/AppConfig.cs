﻿using H.Necessaire.Runtime;
using System;
using System.IO;
using System.Reflection;

namespace H.Necessaire.MQ.CLI
{
    static class AppConfig
    {
        const string srcFolderRelativePath = "/Src/H.Necessaire.MQ/";

        public static ImAnApiWireup WithDefaultRuntimeConfig(this ImAnApiWireup wireup)
        {
            return
                wireup
                .With(x => x.Register<RuntimeConfig>(() => new RuntimeConfig
                {
                    Values = [
                        "NuSpecRootFolderPath".ConfigWith(GetCodebaseFolderPath()),
                        "QdActions".ConfigWith(
                            "MaxProcessingAttempts".ConfigWith($"{2}"),
                            "RabbitMQ".ConfigWith(
                                "HostName".ConfigWith(ReadConfigFromFile("RabbitMqHost.cfg.txt"))
                                ,"VirtualHost".ConfigWith(ReadConfigFromFile("RabbitMqVirtualHost.cfg.txt"))
                                ,"UserName".ConfigWith(ReadConfigFromFile("RabbitMqUser.cfg.txt"))
                                ,"Password".ConfigWith(ReadConfigFromFile("RabbitMqPass.cfg.txt"))
                            ),
                            "Azure".ConfigWith(
                                "ServiceBus".ConfigWith(
                                    "ConnectionString".ConfigWith(ReadConfigFromFile("AzureServiceBusConnectionString.cfg.txt"))
                                    , "QueueName".ConfigWith(ReadConfigFromFile("AzureServiceBusQueueName.cfg.txt"))
                                )
                            )
                        )
                    ],
                }));
            ;
        }

        private static string ReadConfigFromFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
                return null;

            string result = null;

            new Action(() =>
            {
                result = File.ReadAllText(fileInfo.FullName);
            })
            .TryOrFailWithGrace(onFail: ex => result = null);

            return result;
        }

        private static string GetCodebaseFolderPath()
        {
            string codeBase = Assembly.GetExecutingAssembly()?.Location ?? string.Empty;
            UriBuilder uri = new UriBuilder(codeBase);
            string dllPath = Uri.UnescapeDataString(uri.Path);
            int srcFolderIndex = dllPath.ToLowerInvariant().IndexOf(srcFolderRelativePath, StringComparison.InvariantCultureIgnoreCase);
            if (srcFolderIndex < 0)
                return string.Empty;
            string srcFolderPath = Path.GetDirectoryName(dllPath.Substring(0, srcFolderIndex + srcFolderRelativePath.Length)) ?? string.Empty;
            return srcFolderPath;
        }
    }
}
