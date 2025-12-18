using H.Necessaire.MQ.Abstractions;
using H.Necessaire.Serialization;
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.NamedPipe.Concrete
{
    [ID("NamedPipe")]
    [Alias("named-pipe", "np")]
    internal class NamedPipeHmqExternalEventListener : ImAnHmqExternalEventListener, ImADependency, IDisposable
    {
        const string defaultPipeName = "hmq";
        const uint bufferSize = 512;
        static readonly TimeSpan pipeReadPause = TimeSpan.FromSeconds(.15);
        string pipeName = defaultPipeName;
        bool isRunning = false;
        ImAnHmqEventRiser internalEventRiser;
        ImALogger logger;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            ConfigNode config
                = dependencyProvider
                .GetRuntimeConfig()
                ?.Get("HMQ")
                ?.Get("NamedPipe")
                ;
            string configPipeName = config?.Get("PipeName")?.ToString();
            if (!configPipeName.IsEmpty())
                pipeName = configPipeName;

            internalEventRiser = dependencyProvider.Build<ImAnHmqEventRiser>("internal");
            logger = dependencyProvider.GetLogger<NamedPipeHmqExternalEventListener>();
        }

        public Task<OperationResult> Start()
        {
            if (isRunning)
                return OperationResult.Win().AsTask();

            isRunning = true;

            StartNewServerAndWaitForClientConnection();

            return OperationResult.Win().AsTask();
        }

        public Task<OperationResult> Stop()
        {
            if (!isRunning)
                return OperationResult.Win().AsTask();

            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            isRunning = false;
            return OperationResult.Win().AsTask();
        }

        public void Dispose()
        {
            new Action(() =>
            {
                Stop().ConfigureAwait(false).GetAwaiter().GetResult();

            }).TryOrFailWithGrace();
        }

        void StartNewServerAndWaitForClientConnection()
        {
            Task.Run(async () =>
                {
                    using (NamedPipeServerStream namedPipeServerStream = new NamedPipeServerStream(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances))
                    {
                        await namedPipeServerStream.WaitForConnectionAsync(cancellationTokenSource.Token);

                        StartNewServerAndWaitForClientConnection();

                        StringBuilder messageBuilder = new StringBuilder();

                        byte[] readBuffer = new byte[bufferSize];
                        while (namedPipeServerStream.IsConnected && !cancellationTokenSource.IsCancellationRequested)
                        {
                            int readBytes = await namedPipeServerStream.ReadAsync(readBuffer, 0, readBuffer.Length, cancellationTokenSource.Token);
                            if (readBytes == 0)
                            {
                                await Task.Delay(pipeReadPause, cancellationTokenSource.Token);
                                if (cancellationTokenSource.IsCancellationRequested)
                                    break;
                                else
                                    continue;
                            }

                            string messageChunk = Encoding.UTF8.GetString(readBuffer, 0, readBytes);
                            messageBuilder.Append(messageChunk);
                        }

                        namedPipeServerStream.Close();

                        string message = messageBuilder.ToString();

                        await OnMessageReceived(message);
                    }
                }
                ,
                cancellationTokenSource.Token
            );
        }

        private async Task OnMessageReceived(string message)
        {
            await
                new Func<Task>(async () =>
                {
                    string hmqEventAsJsonString = message;
                    HmqEvent hmqEvent = hmqEventAsJsonString.TryJsonToObject<HmqEvent>().ThrowOnFailOrReturn();
                    await internalEventRiser.Raise(hmqEvent);

                })
                .TryOrFailWithGrace(onFail: async ex =>
                {
                    await logger.LogError($"Error occurred while trying to process received event from Named Pipe; probably the payload is not an HmqEvent and therefor cannot be parsed.{Environment.NewLine}Body: {message ?? "~~N/A~~"}{Environment.NewLine}Message: {ex.Message}", ex, message as object);
                });
        }
    }
}
