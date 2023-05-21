using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        private const string DaemonClientName = "vernuntii-msbuild-daemon-client";
        private const int ConnectTimeout = 8000;
        private const int ReconnectTimeout = 4000;

        private static readonly Dictionary<ConsoleProcessExecutionArguments, VernuntiiDaemon> s_consoleExecutableAssociatedDaemonDictionary;
        private static bool s_areDaemonsTerminated;

        static ConsoleProcessExecutor() => s_consoleExecutableAssociatedDaemonDictionary = new();

        private static VernuntiiDaemon CreateDaemon(ConsoleProcessExecutionArguments executionArguments, TaskLoggingHelper logger)
        {
            var loggerHolder = new TaskLoggingHelperHolder() { Logger = logger };
            var receivingPipeName = DaemonClientName + Guid.NewGuid().ToString();
            var receivingPipe = new NamedPipeServerStream(receivingPipeName, PipeDirection.Out);
            return new VernuntiiDaemon(executionArguments, receivingPipeName, receivingPipe, logger);
        }

        private static VernuntiiDaemon GetOrCreateDaemon(ConsoleProcessExecutionArguments executionArguments, TaskLoggingHelper logger)
        {
            lock (s_consoleExecutableAssociatedDaemonDictionary) {
                if (s_areDaemonsTerminated) {
                    throw new InvalidOperationException("All daemons are terminated");
                }

                if (s_consoleExecutableAssociatedDaemonDictionary.TryGetValue(executionArguments, out var daemon)) {
                    return daemon;
                }

                daemon = CreateDaemon(executionArguments, logger);
                s_consoleExecutableAssociatedDaemonDictionary.Add(executionArguments, daemon);
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
                using var sendingPipe = daemon.CreateConnectedSendingPipe();
                var receivingPipeNameBytes = Encoding.ASCII.GetBytes(daemon.ReceivingPipeName);
                sendingPipe.Write(receivingPipeNameBytes, 0, receivingPipeNameBytes.Length);
                sendingPipe.WriteByte(NextVersionDaemonProtocolDefaults.Delimiter);
                sendingPipe.Flush();

                daemon.ReceivingPipe.WaitForConnection();
                try {
                    nextVersionMessage = new MemoryStream();
                    nextVersionMessageType = daemon.NextVersionPipeReader.ReadNextVersionAsync(nextVersionMessage).GetAwaiter().GetResult();
                } finally {
                    daemon.ReceivingPipe.Disconnect();
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

        private sealed class TaskLoggingHelperHolder
        {
            public TaskLoggingHelper? Logger { get; set; }
        }

        private sealed class VernuntiiDaemon : IDisposable
        {
            public ConsoleProcessExecutionArguments ExecutionArguments { get; }
            public string ReceivingPipeName { get; }
            public NamedPipeServerStream ReceivingPipe { get; }
            public NextVersionPipeReader NextVersionPipeReader { get; }
            public object ProtocolSynchronizationLock { get; } = new();
            public TaskLoggingHelper Logger { get; }

            public VernuntiiDaemon(
                ConsoleProcessExecutionArguments executionArguments,
                string receivingPipeName,
                NamedPipeServerStream receivingPipe,
                TaskLoggingHelper logger)
            {
                ExecutionArguments = executionArguments;
                ReceivingPipeName = receivingPipeName;
                ReceivingPipe = receivingPipe;
                NextVersionPipeReader = NextVersionPipeReader.Create(receivingPipe);
                Logger = logger;
            }

            public NamedPipeClientStream CreateConnectedSendingPipe()
            {
                var executonArgumentsHash = ExecutionArguments.GetHashCode().ToString();
                var sendingPipeName = "vernuntii-daemon" + executonArgumentsHash;
                var sendingPipe = new NamedPipeClientStream(NextVersionDaemonProtocolDefaults.ServerName, sendingPipeName, PipeDirection.Out);

                var daemonSpawnerMutexName = NextVersionDaemonProtocolDefaults.MutexPrefix + DaemonClientName + executonArgumentsHash;
                using var daemonSpawnerMutex = new Mutex(true, daemonSpawnerMutexName, out var isDaemonSpawnerMutexNotTaken);

                try {
                    void ConnectToDaemon()
                    {
                        var processArguments = ExecutionArguments.Concatenation +
                            " --daemon" +
                            $" {sendingPipeName}" +
                            " --daemon-timeout 300";

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

                        sendingPipe.Connect(ConnectTimeout);
                        Logger.LogMessage($"The {nameof(Vernuntii)} daemon ({process.Id}) has been spawned");
                    }

                    if (!isDaemonSpawnerMutexNotTaken) {
                        daemonSpawnerMutex.WaitOne();
                    }

                    var daemonSpawnedMutexName = NextVersionDaemonProtocolDefaults.MutexPrefix + sendingPipeName;
                    var isDaemonSpawnedMutexTaken = Mutex.TryOpenExisting(daemonSpawnedMutexName, out var mutex);
                    mutex?.Dispose();

                    if (!isDaemonSpawnedMutexTaken) {
                        ConnectToDaemon();
                    } else {
                        try {
                            sendingPipe.Connect(ReconnectTimeout);
                        } catch (TimeoutException) {
                            Logger.LogMessage($"Connecting to {nameof(Vernuntii)} daemon failed due to timeout, so we spawn a new daemon");
                            ConnectToDaemon();
                        }
                    }

                    Logger.LogMessage($"Connected to the {nameof(Vernuntii)} daemon via named pipe ({sendingPipeName})");
                } finally {
                    daemonSpawnerMutex.ReleaseMutex();
                }

                return sendingPipe;
            }

            public void Dispose() =>
                ReceivingPipe.Dispose();
        }
    }
}
