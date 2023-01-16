using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using Kenet.SimpleProcess;
using Microsoft.Build.Utilities;
using Vernuntii.Plugins;

namespace Vernuntii.Console.MSBuild
{
    internal class ConsoleProcessExecutor
    {
        private static readonly Dictionary<ConsoleProcessExecutionArguments, VernuntiiDaemon> s_consoleExecutableAssociatedDaemonDictionary;
        private static bool s_areDaemonsTerminated;

        static ConsoleProcessExecutor() => s_consoleExecutableAssociatedDaemonDictionary = new();

        private static VernuntiiDaemon CreateDaemon(ConsoleProcessExecutionArguments executionArguments, TaskLoggingHelper logger)
        {
            var sendingPipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            var receivingPipe = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            var processArguments = $"{executionArguments.CreateConsoleArguments()}" +
                " --daemon" +
                $" {sendingPipe.GetClientHandleAsString()}" +
                $" {receivingPipe.GetClientHandleAsString()}" +
                " --daemon-timeout 300";
            var startInfo = new ConsoleProcessStartInfo(executionArguments.ConsoleExecutablePath, processArguments);
            var loggerHolder = new TaskLoggingHelperHolder() { Logger = logger };

            var processExecution = ProcessExecutorBuilder.CreateDefault(startInfo)
                //.AddErrorWriter(bytes => loggerHolder.Logger?.LogMessage(Encoding.UTF8.GetString(bytes, bytes.Length)))
                .Run();

            sendingPipe.DisposeLocalCopyOfClientHandle();
            receivingPipe.DisposeLocalCopyOfClientHandle();
            return new VernuntiiDaemon(processExecution, sendingPipe, receivingPipe, loggerHolder);
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

        private static void TerminateDaemon(ConsoleProcessExecutionArguments executionArguments)
        {
            lock (s_consoleExecutableAssociatedDaemonDictionary) {
                if (s_consoleExecutableAssociatedDaemonDictionary.TryGetValue(executionArguments, out var daemon)) {
                    try {
                        daemon.Dispose();
                    } finally {
                        s_consoleExecutableAssociatedDaemonDictionary.Remove(executionArguments);
                    }
                }
            }
        }

        private readonly TaskLoggingHelper _logger;

        public ConsoleProcessExecutor(TaskLoggingHelper logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
        public GitVersionPresentation Execute(ConsoleProcessExecutionArguments arguments, bool isNextTry)
        {
            var daemon = GetOrCreateDaemon(arguments, _logger);
            using var logProcessExit = daemon.Process.Exited.Register(() => _logger.LogError($"The {nameof(Vernuntii)} daemon process exited unexpectely"));

            MemoryStream nextVersionMessage;
            NextVersionDaemonProtocolMessageType nextVersionMessageType;

            lock (daemon.ProtocolSynchronizationLock) {
                try {
                    daemon.LoggetHolder.Logger = _logger;
                    var sendingPipe = daemon.SendingPipe;
                    sendingPipe.WriteByte(NextVersionDaemonProtocolDefaults.Delimiter);
                    sendingPipe.Flush();

                    nextVersionMessage = new MemoryStream();
                    nextVersionMessageType = daemon.NextVersionPipeReader.ReadNextVersionAsync(nextVersionMessage).GetAwaiter().GetResult();
                } catch {
                    if (!isNextTry && daemon.Process.IsCompleted) {
                        TerminateDaemon(arguments);
                        return Execute(arguments, isNextTry: true);
                    }

                    throw;
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

        [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
        public GitVersionPresentation Execute(ConsoleProcessExecutionArguments arguments) =>
            Execute(arguments, isNextTry: false);

        private sealed class TaskLoggingHelperHolder
        {
            public TaskLoggingHelper? Logger { get; set; }
        }

        private sealed class VernuntiiDaemon : IDisposable
        {
            public ProcessExecution Process { get; }
            public AnonymousPipeServerStream SendingPipe { get; }
            public NextVersionPipeReader NextVersionPipeReader { get; }
            public object ProtocolSynchronizationLock { get; } = new();
            public TaskLoggingHelperHolder LoggetHolder { get; }

            private readonly AnonymousPipeServerStream _receivingPipe;

            public VernuntiiDaemon(
                ProcessExecution process,
                AnonymousPipeServerStream sendingPipe,
                AnonymousPipeServerStream receivingPipe,
                TaskLoggingHelperHolder loggetHolder)
            {
                Process = process;
                SendingPipe = sendingPipe;
                _receivingPipe = receivingPipe;
                NextVersionPipeReader = NextVersionPipeReader.Create(receivingPipe);
                LoggetHolder = loggetHolder;
            }

            public void Dispose()
            {
                try {
                    Process.Kill();
                } catch {
                    ; // Ignore on purpose
                }

                Process.Dispose();
                _receivingPipe.Dispose();
                SendingPipe.Dispose();
            }
        }
    }
}
