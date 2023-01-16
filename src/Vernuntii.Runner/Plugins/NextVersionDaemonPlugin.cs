using System.Buffers;
using System.CommandLine;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;
using Vernuntii.Runner;

namespace Vernuntii.Plugins;

/// <summary>
/// Similiar to <see cref="NextVersionPlugin"/> but daemonized. Must appear after <see cref="NextVersionPlugin"/>.
/// </summary>
[ImportPlugin<NextVersionDaemonClientPlugin>(TryRegister = true)]/// 
internal class NextVersionDaemonPlugin : Plugin
{
    private const string TwoAnonymousPipesAreRequiredMessage = "Two anonymous pipe handles are required: the first for receiving and the second for sending";

    internal bool AllowEditingPipeHandles { get; set; }

    private readonly VernuntiiRunner _runner;
    private readonly INextVersionPlugin _nextVersionPlugin;
    private readonly ILogger<NextVersionDaemonPlugin> _logger;

    private readonly Option<string> _daemonOption = new Option<string>("--daemon", $"Allows to start {nameof(Vernuntii)} as daemon." +
        $" {TwoAnonymousPipesAreRequiredMessage}.") {
        //Arity = new ArgumentArity(minimumNumberOfValues:21, maximumNumberOfValues: 2),
        //AllowMultipleArgumentsPerToken = true,
        IsHidden = true
    };

    private readonly Option<bool> _daemonStartupVersionOption = new Option<bool>("--daemon-produces-version-at-startup", $"Allows {nameof(Vernuntii)} to produce the next-version at startup.") {
        IsHidden = true
    };

    private readonly Option<int> _daemonTimeout = new Option<int>("--daemon-timeout", () => Timeout.Infinite, "After the timeout (seconds), the daemon will be terminated. If -1, no timeout is used.") {
        IsHidden = true
    };

    public NextVersionDaemonPlugin(VernuntiiRunner runner, INextVersionPlugin nextVersionPlugin, ILogger<NextVersionDaemonPlugin> logger)
    {
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _nextVersionPlugin = nextVersionPlugin ?? throw new ArgumentNullException(nameof(nextVersionPlugin));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _nextVersionPlugin.Command.Add(_daemonOption);
        _nextVersionPlugin.Command.Add(_daemonStartupVersionOption);
        _nextVersionPlugin.Command.Add(_daemonTimeout);
    }

    protected override void OnExecution()
    {
        // We skip the the very first "next version"-invocation.
        Events.Once(LifecycleEvents.BeforeEveryRun)
            .Zip(Events.Once(CommandLineEvents.ParsedCommandLineArguments))
            .Zip(Events.Once(NextVersionEvents.OnInvokeNextVersionCommand))
            .Where(_ => _nextVersionPlugin.Command.IsSeatTaken)
            .Subscribe(async result => {
                var ((lifecycleContext, parseResult), nextVersionCommandInvocation) = result;
                var receivingPipeHandle = parseResult.GetValueForOption(_daemonOption);

                if (!AllowEditingPipeHandles && string.IsNullOrWhiteSpace(receivingPipeHandle)) {
                    // The option was not specified
                    return;
                }

                var pipes = new NextVersionDaemonEvents.Pipes(
                    receivingPipeHandle: receivingPipeHandle);

                if (AllowEditingPipeHandles) {
                    await Events.EmitAsync(NextVersionDaemonEvents.OnEditPipes, pipes);
                }

                pipes.CheckPipes();
                _logger.LogInformation($"{nameof(Vernuntii)} has been started as daemon");
                var daemonProducesVersionAtStartup = parseResult.GetValueForOption(_daemonStartupVersionOption);
                nextVersionCommandInvocation.IsHandled = !daemonProducesVersionAtStartup;
                var daemonTimeout = parseResult.GetValueForOption(_daemonTimeout); // Seconds

                lifecycleContext.NewBackgroundTask(async () => {
                    var mutexName = NextVersionDaemonProtocolDefaults.MutexPrefix + pipes.ReceivingPipeName;
                    using var mutex = new Mutex(initiallyOwned: true, mutexName, out var isMutexNotTaken);

                    if (!isMutexNotTaken) {
                        // We cannot have two daemons with same receiving pipe names living along.
                        Environment.Exit(0);
                    }

                    Timer? timer = null;

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    void ResetTimer() => timer?.Change(dueTime: daemonTimeout, period: Timeout.Infinite);

                    if (daemonTimeout >= 0) {
                        daemonTimeout *= 1000; // Milliseconds

                        timer = new Timer(_ => {
                            // TODO:
                            //var error = new NextVersionApiException("Daemon terminated due to timeout");
                            //pipe.Reader.Complete(error);
                            //pipe.Writer.Complete(error);
                            Environment.Exit(0);
                        });

                        ResetTimer();
                    }

                    var receivingPipe = new NamedPipeServerStream(pipes.ReceivingPipeName, PipeDirection.In);

                    while (true) {
                        var pipe = new Pipe();

                        try {
                            if (pipes.WaitForConnectionCancellatonToken.IsCancellationRequested) {
                                break;
                            }

                            await receivingPipe.WaitForConnectionAsync(pipes.WaitForConnectionCancellatonToken).WaitAsync(pipes.WaitForConnectionCancellatonToken);
                        } catch (OperationCanceledException) {
                            break;
                        }

                        using var cancellationTokenSource = new CancellationTokenSource();
                        using var readerAwaiter = new AutoResetEvent(initialState: true);

                        async Task LogErrorAsync(Func<Task> taskProvider)
                        {
                            try {
                                await taskProvider();
                            } catch (Exception error) {
                                _logger.LogError(error, "An error occured inside the daemon");
                            }
                        }

                        await Task.WhenAll(
                            LogErrorAsync(() => WriteToPipeAsync(pipe.Writer, cancellationTokenSource.Token)),
                            LogErrorAsync(() => ReadFromPipeAsync(pipe.Reader, cancellationTokenSource.Cancel)));

                        receivingPipe.Disconnect();

                        async Task WriteToPipeAsync(PipeWriter writer, CancellationToken cancellationToken)
                        {
                            while (true) {
                                try {
                                    readerAwaiter.WaitOne();

                                    if (cancellationToken.IsCancellationRequested) {
                                        break;
                                    }

                                    var pipeBuffer = writer.GetMemory();
                                    var bytesRead = await receivingPipe.ReadAsync(pipeBuffer);

                                    if (bytesRead == 0) {
                                        break;
                                    }

                                    writer.Advance(bytesRead);
                                } catch {
                                    break;
                                }

                                var result = await writer.FlushAsync();

                                if (result.IsCompleted) {
                                    break;
                                }
                            }

                            await writer.CompleteAsync().ConfigureAwait(false);
                        }

                        async Task ReadFromPipeAsync(PipeReader reader, Action complete)
                        {
                            static bool TryReadSendingPipeHandle(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> sendingPipeNameBuffer)
                            {
                                var delimiterPosition = buffer.PositionOf(NextVersionDaemonProtocolDefaults.Delimiter);

                                if (delimiterPosition == null) {
                                    sendingPipeNameBuffer = ReadOnlySequence<byte>.Empty;
                                    return false;
                                }

                                sendingPipeNameBuffer = buffer.Slice(0, delimiterPosition.Value);
                                buffer = buffer.Slice(buffer.GetPosition(offset: 1, delimiterPosition.Value));
                                return true;
                            }

                            Exception? capturedError = null;

                            while (true) {
                                var result = await reader.ReadAsync();
                                var buffer = result.Buffer;

                                var couldReadSendingPipeHandle = TryReadSendingPipeHandle(ref buffer, out var sendingPipeNameBuffer);

                                if (couldReadSendingPipeHandle) {
                                    do {
                                        ResetTimer();
                                        NextVersionResult nextVersionResult;

                                        var sendingPipeName = Encoding.ASCII.GetString(sendingPipeNameBuffer);
                                        await using var sendingPipe = new NamedPipeClientStream(NextVersionDaemonProtocolDefaults.ServerName, sendingPipeName, PipeDirection.Out);
                                        await sendingPipe.ConnectAsync();

                                        try {
                                            nextVersionResult = await _runner.NextVersionAsync().ConfigureAwait(false);
                                        } catch (Exception error) {
                                            var errorMessageBytes = Encoding.UTF8.GetBytes(error.ToString());
                                            sendingPipe.WriteByte(NextVersionDaemonProtocolDefaults.Failure); // Failure
                                            await sendingPipe.WriteAsync(errorMessageBytes).ConfigureAwait(false);
                                            await sendingPipe.FlushAsync().ConfigureAwait(false);
                                            sendingPipe.WriteByte(NextVersionDaemonProtocolDefaults.Delimiter); // Delimiter
                                            capturedError = error;
                                            break;
                                        }

                                        var nextVersionBytes = Encoding.UTF8.GetBytes(nextVersionResult.VersionCacheString);
                                        sendingPipe.WriteByte(NextVersionDaemonProtocolDefaults.Success); // Success
                                        await sendingPipe.WriteAsync(nextVersionBytes).ConfigureAwait(false);
                                        sendingPipe.WriteByte(NextVersionDaemonProtocolDefaults.Delimiter); // Delimiter
                                        await sendingPipe.FlushAsync().ConfigureAwait(false);
                                    } while (TryReadSendingPipeHandle(ref buffer, out sendingPipeNameBuffer));

                                    // EXPERIMENTAL:
                                    break;
                                }

                                // Tell the PipeReader how much of the buffer has been consumed.
                                reader.AdvanceTo(buffer.Start, buffer.End);

                                // Stop reading if there's no more data coming.
                                if (result.IsCompleted) {
                                    break;
                                } else {
                                    // Allow writer to read next
                                    readerAwaiter.Set();
                                }
                            }

                            await reader.CompleteAsync().ConfigureAwait(false);
                            complete();
                            readerAwaiter.Set();
                        }
                    }
                });
            });
    }
}
