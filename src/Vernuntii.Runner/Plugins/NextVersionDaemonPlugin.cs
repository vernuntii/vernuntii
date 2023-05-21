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
[ImportPlugin<NextVersionDaemonClientPlugin>(TryRegister = true)]
internal class NextVersionDaemonPlugin : Plugin
{
    private const string AnonymousPipeIsRequiredMessage = "The daemon pipe server name is required: The daemon starts a pipe server that waits for connection after connection.";

    internal bool AllowEditingPipeHandles { get; set; }

    private readonly VernuntiiRunner _runner;
    private readonly INextVersionPlugin _nextVersionPlugin;
    private readonly ILogger<NextVersionDaemonPlugin> _logger;

    private readonly Option<string> _daemonOption = new Option<string>("--daemon", $"Allows to start {nameof(Vernuntii)} as daemon." +
        $" {AnonymousPipeIsRequiredMessage}.") {
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
                    daemonPipeServerName: receivingPipeHandle);

                if (AllowEditingPipeHandles) {
                    await Events.EmitAsync(NextVersionDaemonEvents.OnEditPipes, pipes);
                }

                pipes.CheckPipes();
                _logger.LogInformation($"{nameof(Vernuntii)} has been started as daemon");
                var daemonProducesVersionAtStartup = parseResult.GetValueForOption(_daemonStartupVersionOption);
                nextVersionCommandInvocation.IsHandled = !daemonProducesVersionAtStartup;
                var daemonTimeout = parseResult.GetValueForOption(_daemonTimeout); // Seconds

                lifecycleContext.NewBackgroundTask(async () => {
                    var mutexName = NextVersionDaemonProtocolDefaults.MutexPrefix + pipes.DaemonPipeServerName;
                    using var mutex = new Mutex(initiallyOwned: true, mutexName, out var isMutexNotTaken);

                    // If taken ..
                    if (!isMutexNotTaken) {
                        // .. we stop the daemon immediatelly
                        return; // Environment.Exit(0);
                    }

                    var daemonCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(pipes.WaitForConnectionCancellatonToken);
                    Timer? timer = null;

                    if (daemonTimeout >= 0) {
                        daemonTimeout *= 1000; // Milliseconds

                        timer = new Timer(_ => {
                            // TODO:
                            //var error = new NextVersionApiException("Daemon terminated due to timeout");
                            //pipe.Reader.Complete(error);
                            //pipe.Writer.Complete(error);
                            daemonCancellationTokenSource.Cancel();
                        });

                        ResetTimer();
                    }

                    using var daemonPipeServer = new NamedPipeServerStream(pipes.DaemonPipeServerName, PipeDirection.In, maxNumberOfServerInstances: 1, PipeTransmissionMode.Byte);

                    while (true) {
                        var pipe = new Pipe();

                        try {
                            if (daemonCancellationTokenSource.IsCancellationRequested) {
                                break;
                            }

                            await daemonPipeServer.WaitForConnectionAsync(daemonCancellationTokenSource.Token).WaitAsync(daemonCancellationTokenSource.Token).ConfigureAwait(false);
                        } catch (OperationCanceledException) {
                            break;
                        }

                        using var writeCancellationTokenSource = new CancellationTokenSource();
                        using var readerAwaiter = new AutoResetEvent(initialState: true);

                        try {
                            await Task.WhenAll(
                                WriteToPipeAsync(pipe.Writer, writeCancellationTokenSource.Token),
                                ReadFromPipeAsync(pipe.Reader, writeCancellationTokenSource.Cancel));

                            async Task WriteToPipeAsync(PipeWriter writer, CancellationToken cancellationToken)
                            {
                                while (true) {
                                    try {
                                        readerAwaiter.WaitOne();

                                        if (cancellationToken.IsCancellationRequested) {
                                            break;
                                        }

                                        var pipeBuffer = writer.GetMemory();
                                        var bytesRead = await daemonPipeServer.ReadAsync(pipeBuffer, cancellationToken).ConfigureAwait(false);

                                        if (bytesRead == 0) {
                                            break;
                                        }

                                        writer.Advance(bytesRead);
                                    } catch {
                                        break;
                                    }

                                    var result = await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

                                    if (result.IsCompleted) {
                                        break;
                                    }
                                }

                                await writer.CompleteAsync().ConfigureAwait(false);
                            }

                            async Task ReadFromPipeAsync(PipeReader reader, Action completionCallback)
                            {
                                Exception? capturedError = null;

                                while (true) {
                                    var result = await reader.ReadAsync();
                                    var buffer = result.Buffer;

                                    if (TryReadSendingPipeHandle(ref buffer, out var daemonClientPipeServerNameBuffer)) {
                                        do {
                                            ResetTimer();
                                            NextVersionResult nextVersionResult;

                                            var daemonClientPipeServerName = Encoding.ASCII.GetString(daemonClientPipeServerNameBuffer);
                                            await using var sendingPipe = new NamedPipeClientStream(NextVersionDaemonProtocolDefaults.ServerName, daemonClientPipeServerName, PipeDirection.Out);
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
                                        } while (TryReadSendingPipeHandle(ref buffer, out daemonClientPipeServerNameBuffer));

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
                                }

                                await reader.CompleteAsync().ConfigureAwait(false);
                                completionCallback();
                                readerAwaiter.Set();
                            }
                        } catch (Exception error) {
                            _logger.LogError(error, "An error occured inside the daemon");
                        } finally {
                            daemonPipeServer.Disconnect();
                        }
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    void ResetTimer() => timer?.Change(dueTime: daemonTimeout, period: Timeout.Infinite);
                });
            });
    }
}
