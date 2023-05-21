using System;
using System.Diagnostics;
using System.Globalization;
using System.IO.Hashing;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Utilities;
using Vernuntii.Plugins;

namespace Vernuntii.Console.MSBuild
{
    internal sealed class VernuntiiDaemon : IDisposable
    {
        private const int DaemonPipeServerConnectTimeout = 8000;
        private const int DaemonPipeServerReconnectTimeout = 4000;

        public ConsoleProcessExecutionArguments ExecutionArguments { get; }
        public string DaemonClientPipeServerName { get; }
        public NamedPipeServerStream DaemonClientPipeServer { get; }
        public NextVersionPipeReader NextVersionPipeReader { get; }
        public Semaphore ProtocolSynchronizationLock { get; } = new(1, 1);
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

        public async Task<NamedPipeClientStream> CreateOutgoingDaemonPipeClientAsync()
        {
            var daemonNameSuffix = string.IsNullOrWhiteSpace(ExecutionArguments.CacheId)
                ? "-no_cache_id"
                : "-by_cache_id:" + ExecutionArguments.CacheId?.ToCrc32HexString();

            var daemonPipeServerName = "vernuntii_daemon" + daemonNameSuffix;
            var daemonPipeServer = new NamedPipeClientStream(NextVersionDaemonProtocolDefaults.ServerName, daemonPipeServerName, PipeDirection.Out, PipeOptions.Asynchronous);

            var daemonSpawnerMutexName = NextVersionDaemonProtocolDefaults.MutexPrefix + ConsoleProcessExecutor.DaemonClientPipeServerName + daemonNameSuffix;
            await using var daemonSpawnerMutex = new AsyncMutex(daemonSpawnerMutexName);
            await daemonSpawnerMutex.AcquireAsync(10 * 1000).ConfigureAwait(false);

            try {
                void ConnectToDaemon()
                {
                    var processArguments = "--daemonize " +
                        ExecutionArguments.Concatenation +
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

                var daemonSpawnedMutexName = NextVersionDaemonProtocolDefaults.GetDaemonServerMutexName(daemonPipeServerName);
                var isDaemonSpawnedMutexTaken = Mutex.TryOpenExisting(daemonSpawnedMutexName, out var mutex);
                mutex?.Dispose();

                if (!isDaemonSpawnedMutexTaken) {
                    ConnectToDaemon();
                } else {
                    try {
                        await daemonPipeServer.ConnectAsync(DaemonPipeServerReconnectTimeout).ConfigureAwait(false);
                    } catch (TimeoutException) {
                        Logger.LogMessage($"Connecting to {nameof(Vernuntii)} daemon failed due to timeout, so we spawn a new daemon");
                        // ISSUE: recursive call
                        ConnectToDaemon();
                    }
                }

                Logger.LogMessage($"Connected to the {nameof(Vernuntii)} daemon via named pipe ({daemonPipeServerName})");
            } finally {
                await daemonSpawnerMutex.ReleaseAsync().ConfigureAwait(false);
            }

            return daemonPipeServer;
        }

        public void Dispose() =>
            DaemonClientPipeServer.Dispose();
    }
}
