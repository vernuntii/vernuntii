using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using Microsoft.Build.Utilities;
using Vernuntii.Plugins;

namespace Vernuntii.Console.MSBuild
{
    internal class ConsoleProcessExecutor
    {
        private const string DaemonClientPipeServerName = "vernuntii-msbuild-daemon-client";
        private const int DaemonPipeServerConnectTimeout = 8000;
        private const int DaemonPipeServerReconnectTimeout = 4000;

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

        [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
        public GitVersionPresentation Execute(ConsoleProcessExecutionArguments arguments)
        {
            var daemon = GetOrCreateDaemon(arguments, _logger);

            MemoryStream nextVersionMessage;
            NextVersionDaemonProtocolMessageType nextVersionMessageType;

            lock (daemon.ProtocolSynchronizationLock) {
                using var outgoingDaemonPipeClient = daemon.CreateOutgoingDaemonPipeClient();
                var daemonClientPipeServerNameBytes = Encoding.ASCII.GetBytes(daemon.DaemonClientPipeServerName);
                outgoingDaemonPipeClient.Write(daemonClientPipeServerNameBytes, 0, daemonClientPipeServerNameBytes.Length);
                outgoingDaemonPipeClient.WriteByte(NextVersionDaemonProtocolDefaults.Delimiter);
                outgoingDaemonPipeClient.Flush();

                daemon.DaemonClientPipeServer.WaitForConnection();
                try {
                    nextVersionMessage = new MemoryStream();
                    nextVersionMessageType = daemon.NextVersionPipeReader.ReadNextVersionAsync(nextVersionMessage).GetAwaiter().GetResult();
                } finally {
                    daemon.DaemonClientPipeServer.Disconnect();
                }
            }

            daemon.NextVersionPipeReader.ValidateNextVersion(nextVersionMessageType, nextVersionMessage.GetBuffer);

            try {
                nextVersionMessage.Position = 0;
                return JsonSerializer.Deserialize<GitVersionPresentation>(nextVersionMessage) ?? throw new InvalidOperationException();
            } catch (Exception error) {
                throw new NextVersionApiException($"The {nameof(Vernuntii)} daemon process could not produce a valid deserializable JSON object: {error}");
            }
        }

        private sealed class VernuntiiDaemon : IDisposable
        {
            public ConsoleProcessExecutionArguments ExecutionArguments { get; }
            public string DaemonClientPipeServerName { get; }
            public NamedPipeServerStream DaemonClientPipeServer { get; }
            public NextVersionPipeReader NextVersionPipeReader { get; }
            public object ProtocolSynchronizationLock { get; } = new();
            public TaskLoggingHelper Logger { get; }

            public VernuntiiDaemon(
                ConsoleProcessExecutionArguments executionArguments,
                string daemonClientPipeServerName,
                NamedPipeServerStream daemonClientPipeServer,
                TaskLoggingHelper logger)
            {
                ExecutionArguments = executionArguments;
                DaemonClientPipeServerName = daemonClientPipeServerName;
                DaemonClientPipeServer = daemonClientPipeServer;
                NextVersionPipeReader = NextVersionPipeReader.Create(daemonClientPipeServer);
                Logger = logger;
            }

            public NamedPipeClientStream CreateOutgoingDaemonPipeClient()
            {
                var executonArgumentsHash = ExecutionArguments.Concatenation.GetHashCode().ToString(CultureInfo.InvariantCulture);
                var daemonPipeServerName = "vernuntii-daemon" + executonArgumentsHash;
                var daemonPipeServer = new NamedPipeClientStream(NextVersionDaemonProtocolDefaults.ServerName, daemonPipeServerName, PipeDirection.Out, PipeOptions.Asynchronous);

                var daemonSpawnerMutexName = NextVersionDaemonProtocolDefaults.MutexPrefix + ConsoleProcessExecutor.DaemonClientPipeServerName + executonArgumentsHash;
                using var daemonSpawnerMutex = new Mutex(true, daemonSpawnerMutexName, out var isDaemonSpawnerMutexNotTaken);

                try {
                    void ConnectToDaemon()
                    {
                        var processArguments = ExecutionArguments.Concatenation +
                            " --daemon" +
                            $" {daemonPipeServerName}" +
                            $" --daemon-timeout {ExecutionArguments.DaemonTimeout}";

                        var startInfo = new ProcessStartInfo(
                            ConsoleProcessStartInfo.GetFileNameOrPath(ExecutionArguments.ConsoleExecutablePath),
                            processArguments) {
                            UseShellExecute = true
                        };

                        Logger.LogMessage($"Spawn new {nameof(Vernuntii)} daemon");
                        using var process = Process.Start(startInfo);

                        void LogDaemonExit() => Logger.LogMessage($"The {nameof(Vernuntii)} daemon ({process.Id}) daemon has exited unexpectly");

                        if (process.HasExited) {
                            LogDaemonExit();
                        } else {
                            process.Exited += (_, _) => LogDaemonExit();
                        }

                        daemonPipeServer.Connect(DaemonPipeServerConnectTimeout);
                        Logger.LogMessage($"The {nameof(Vernuntii)} daemon ({process.Id}) has been spawned");
                    }

                    if (!isDaemonSpawnerMutexNotTaken) {
                        daemonSpawnerMutex.WaitOne();
                    }

                    var daemonSpawnedMutexName = NextVersionDaemonProtocolDefaults.MutexPrefix + daemonPipeServerName;
                    var isDaemonSpawnedMutexTaken = Mutex.TryOpenExisting(daemonSpawnedMutexName, out var mutex);
                    mutex?.Dispose();

                    if (!isDaemonSpawnedMutexTaken) {
                        ConnectToDaemon();
                    } else {
                        try {
                            daemonPipeServer.Connect(DaemonPipeServerReconnectTimeout);
                        } catch (TimeoutException) {
                            Logger.LogMessage($"Connecting to {nameof(Vernuntii)} daemon failed due to timeout, so we spawn a new daemon");
                            // ISSUE: recursive call
                            ConnectToDaemon();
                        }
                    }

                    Logger.LogMessage($"Connected to the {nameof(Vernuntii)} daemon via named pipe ({daemonPipeServerName})");
                } finally {
                    daemonSpawnerMutex.ReleaseMutex();
                }

                return daemonPipeServer;
            }

            public void Dispose() =>
                DaemonClientPipeServer.Dispose();
        }
    }
}
