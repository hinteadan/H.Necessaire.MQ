using H.Necessaire.MQ.Abstractions;
using H.Necessaire.Serialization;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.MQ.Bus.NamedPipe.Concrete
{
    internal class NamedPipeHmqEventRiser : ImAnHmqEventRiser, ImADependency
    {
        const string defaultPipeName = "hmq";
        const string defaultServerName = ".";
        const uint bufferSize = 512;
        string pipeName = defaultPipeName;
        string serverName = defaultServerName;
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

            string configServerName = config?.Get("ServerName")?.ToString();
            if (!configServerName.IsEmpty())
                serverName = configServerName;
        }

        public async Task<OperationResult<ImAnHmqReActor>[]> Raise(HmqEvent hmqEvent)
        {
            if (hmqEvent is null)
                return OperationResult.Win().WithPayload(NamedPipeReActor.Instance).AsArray();

            string messageToSend = hmqEvent.ToJsonObject();

            using (NamedPipeClientStream namedPipeClientStream
                    = new NamedPipeClientStream(serverName, pipeName, PipeDirection.Out, PipeOptions.None, System.Security.Principal.TokenImpersonationLevel.None))
            {
                await namedPipeClientStream.ConnectAsync(cancellationTokenSource.Token);

                byte[] messageToSendAsBytes = Encoding.UTF8.GetBytes(messageToSend);

                await namedPipeClientStream.WriteAsync(messageToSendAsBytes, 0, messageToSendAsBytes.Length, cancellationTokenSource.Token);

                namedPipeClientStream.Close();
            }

            return OperationResult.Win().WithPayload(NamedPipeReActor.Instance).AsArray();
        }
    }
}
