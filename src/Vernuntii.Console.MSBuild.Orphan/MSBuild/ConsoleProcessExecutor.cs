using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Build.Utilities;
using Vernuntii.Plugins;

namespace Vernuntii.Console.MSBuild
{
    internal class ConsoleProcessExecutor
    {
        internal const string DaemonClientPipeServerName = "vernuntii-msbuild-daemon-client";
        internal readonly static TimeSpan s_daemonClientPipeAwaitConnectionTimeout = TimeSpan.FromSeconds(10);
        internal readonly static TimeSpan s_nextVersionReadTimeout = TimeSpan.FromSeconds(5);

        private static readonly Dictionary<ConsoleProcessExecutionArguments, VernuntiiDaemon> s_consoleExecutionArgumentsAssociatedDaemonDictionary;
        private static bool s_areDaemonsTerminated;

        static ConsoleProcessExecutor() => s_consoleExecutionArgumentsAssociatedDaemonDictionary = new();

        private static VernuntiiDaemon CreateDaemon(ConsoleProcessExecutionArguments executionArguments, TaskLoggingHelper logger)
        {
            var daemonClientPipeServerName = DaemonClientPipeServerName + Guid.NewGuid().ToString();
            var daemonClientPipeServer = new NamedPipeServerStream(daemonClientPipeServerName, PipeDirection.In, maxNumberOfServerInstances: 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            return new VernuntiiDaemon(executionArguments, daemonClientPipeServerName, daemonClientPipeServer, logger);
        }

        private static VernuntiiDaemon GetOrCreateDaemon(ConsoleProcessExecutionArguments executionArguments, TaskLoggingHelper logger)
        {
            lock (s_consoleExecutionArgumentsAssociatedDaemonDictionary) {
                if (s_areDaemonsTerminated) {
                    throw new InvalidOperationException("All daemons are terminated");
                }

                if (s_consoleExecutionArgumentsAssociatedDaemonDictionary.TryGetValue(executionArguments, out var daemon)) {
                    return daemon;
                }

                daemon = CreateDaemon(executionArguments, logger);
                s_consoleExecutionArgumentsAssociatedDaemonDictionary.Add(executionArguments, daemon);
                return daemon;
            }
        }

        private readonly TaskLoggingHelper _logger;

        public ConsoleProcessExecutor(TaskLoggingHelper logger) =>
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<GitVersionPresentation> ExecuteAsync(ConsoleProcessExecutionArguments arguments)
        {
            // HINT: Do not use "using"-feature:
            // As this .DLL of this assembly gets reused, we want to reuse the daemon server as well.
            var daemon = GetOrCreateDaemon(arguments, _logger);

            MemoryStream nextVersionMessage;
            NextVersionDaemonProtocolMessageType nextVersionMessageType;

            daemon.ProtocolSynchronizationLock.WaitOne();
            try {
                _logger.LogMessage("Creating outgoing daemon pipe client");
                using var outgoingDaemonPipeClient = await daemon.CreateOutgoingDaemonPipeClientAsync().ConfigureAwait(false);
                var daemonClientPipeServerNameBytes = Encoding.ASCII.GetBytes(daemon.DaemonClientPipeServerName);
                await outgoingDaemonPipeClient.WriteAsync(daemonClientPipeServerNameBytes, 0, daemonClientPipeServerNameBytes.Length).ConfigureAwait(false);
                outgoingDaemonPipeClient.WriteByte(NextVersionDaemonProtocolDefaults.Delimiter);
                await outgoingDaemonPipeClient.FlushAsync().ConfigureAwait(false);

                _logger.LogMessage("Waiting for daemon to connect to our daemon client pipe server");
                await daemon.DaemonClientPipeServer.WaitForConnectionAsync().WaitAsync(s_daemonClientPipeAwaitConnectionTimeout).ConfigureAwait(false);
                try {
                    nextVersionMessage = new MemoryStream();
                    _logger.LogMessage("Reading raw next version");
                    nextVersionMessageType = await daemon.NextVersionPipeReader.ReadNextVersionAsync(nextVersionMessage).WaitAsync(s_nextVersionReadTimeout).ConfigureAwait(false);
                } finally {
                    daemon.DaemonClientPipeServer.Disconnect();
                }
            } finally {
                daemon.ProtocolSynchronizationLock.Release();
            }

            _logger.LogMessage("Validating raw next version");
            daemon.NextVersionPipeReader.ValidateNextVersion(nextVersionMessageType, nextVersionMessage.GetBuffer);

            try {
                nextVersionMessage.Position = 0;
                return JsonSerializer.Deserialize<GitVersionPresentation>(nextVersionMessage) ?? throw new InvalidOperationException();
            } catch (Exception error) {
                throw new NextVersionApiException($"The {nameof(Vernuntii)} daemon process could not produce a valid deserializable JSON object: {error}");
            }
        }
    }
}
