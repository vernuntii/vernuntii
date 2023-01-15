using System;
using System.Collections.Concurrent;
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
        private static ConcurrentDictionary<ConsoleProcessExecutionArguments, VernuntiiRunnerDaemon> s_consoleExecutableAssociatedDaemonDictionary = new();

        private static VernuntiiRunnerDaemon CreateDaemon(ConsoleProcessExecutionArguments executionArguments)
        {
            var sendingPipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            var receivingPipe = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            var processArguments = $"{executionArguments.CreateConsoleArguments()} --daemon {sendingPipe.GetClientHandleAsString()} {receivingPipe.GetClientHandleAsString()}";
            var startInfo = new ConsoleProcessStartInfo(executionArguments.ConsoleExecutablePath, processArguments);
            var processExecution = ProcessExecutorBuilder.CreateDefault(startInfo).Run();
            sendingPipe.DisposeLocalCopyOfClientHandle();
            receivingPipe.DisposeLocalCopyOfClientHandle();
            return new VernuntiiRunnerDaemon(processExecution, sendingPipe, receivingPipe);
        }

        private static VernuntiiRunnerDaemon GetOrCreateDaemon(ConsoleProcessExecutionArguments executionArguments) =>
            s_consoleExecutableAssociatedDaemonDictionary.GetOrAdd(
                executionArguments,
                CreateDaemon);

        private readonly TaskLoggingHelper _logger;

        public ConsoleProcessExecutor(TaskLoggingHelper logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
        public GitVersionPresentation Execute(ConsoleProcessExecutionArguments arguments)
        {
            var daemon = GetOrCreateDaemon(arguments);
            using var logProcessExit = daemon.Process.Exited.Register(() => _logger.LogError($"The {nameof(Vernuntii)} daemon process exited unexpectely"));

            MemoryStream nextVersionMessage;
            NextVersionDaemonProtocolMessageType nextVersionMessageType;

            lock (daemon.ProtocolSynchronization) {
                var sendingPipe = daemon.SendingPipe;
                sendingPipe.WriteByte(NextVersionDaemonProtocolDefaults.Delimiter);
                sendingPipe.Flush();

                nextVersionMessage = new MemoryStream();
                nextVersionMessageType = daemon.NextVersionPipeReader.ReadNextVersionAsync(nextVersionMessage).GetAwaiter().GetResult();
            }

            daemon.NextVersionPipeReader.ValidateNextVersion(nextVersionMessageType, nextVersionMessage.GetBuffer);

            try {
                nextVersionMessage.Position = 0;
                return JsonSerializer.Deserialize<GitVersionPresentation>(nextVersionMessage) ?? throw new InvalidOperationException();
            } catch (Exception error) {
                throw new NextVersionApiException($"The {nameof(Vernuntii)} daemon process could not produce a valid deserializable JSON object: {error}");
            }
        }

        private sealed class VernuntiiRunnerDaemon : IDisposable
        {
            public ProcessExecution Process { get; }
            public AnonymousPipeServerStream SendingPipe { get; }
            public NextVersionPipeReader NextVersionPipeReader { get; }
            public object ProtocolSynchronization { get; } = new();

            private readonly AnonymousPipeServerStream _receivingPipe;

            public VernuntiiRunnerDaemon(ProcessExecution process, AnonymousPipeServerStream sendingPipe, AnonymousPipeServerStream receivingPipe)
            {
                Process = process;
                SendingPipe = sendingPipe;
                _receivingPipe = receivingPipe;
                NextVersionPipeReader = NextVersionPipeReader.Create(receivingPipe);
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
