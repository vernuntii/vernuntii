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
internal class NextVersionDaemonPlugin : Plugin
{
    private const string TwoAnonymousPipesAreRequiredMessage = "Two anonymous pipe handles are required: the first for receiving and the second for sending";

    private readonly VernuntiiRunner _runner;
    private readonly INextVersionPlugin _nextVersionPlugin;
    private readonly ILogger<NextVersionDaemonPlugin> _logger;

    private readonly Option<string[]> _daemonOption = new Option<string[]>("--daemon", $"Allows to start {nameof(Vernuntii)} as daemon." +
        $" {TwoAnonymousPipesAreRequiredMessage}.") {
        Arity = new ArgumentArity(minimumNumberOfValues: 2, maximumNumberOfValues: 2),
        AllowMultipleArgumentsPerToken = true,
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
            .Subscribe(result => {
                var ((lifecycleContext, parseResult), nextVersionCommandInvocation) = result;
                var receivingPipeHandles = parseResult.GetValueForOption(_daemonOption);

                if (receivingPipeHandles is null || receivingPipeHandles.Length == 0) {
                    // The option was not specified
                    return;
                }

                if (receivingPipeHandles.Any(string.IsNullOrWhiteSpace)) {
                    throw new InvalidOperationException(TwoAnonymousPipesAreRequiredMessage);
                }

                _logger.LogInformation($"{nameof(Vernuntii)} has been started as daemon");
                var daemonProducesVersionAtStartup = parseResult.GetValueForOption(_daemonStartupVersionOption);
                nextVersionCommandInvocation.IsHandled = !daemonProducesVersionAtStartup;
                var daemonTimeout = parseResult.GetValueForOption(_daemonTimeout); // Seconds

                lifecycleContext.NewBackgroundTask(async () => {
                    var pipe = new Pipe();
                    Timer? timer = null;

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    void ResetTimer() => timer?.Change(dueTime: 0, period: daemonTimeout);

                    if (daemonTimeout >= 0) {
                        daemonTimeout *= 1000; // Milliseconds

                        timer = new Timer(static state => {
                            var pipe = (Pipe)state!;
                            var error = new NextVersionApiException("Daemon terminated due to timeout");
                            pipe.Reader.Complete(error);
                            pipe.Reader.Complete(error);
                        });

                        ResetTimer();
                    }

                    async Task WriteToPipeAsync(PipeWriter writer)
                    {
                        await using var receivingPipe = new AnonymousPipeClientStream(PipeDirection.In, receivingPipeHandles[0]);

                        while (true) {
                            var pipeBuffer = writer.GetMemory();

                            try {
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

                        await writer.CompleteAsync();
                    }

                    async Task ReadFromPipeAsync(PipeReader reader)
                    {
                        await using var sendingPipe = new AnonymousPipeClientStream(PipeDirection.Out, receivingPipeHandles[1]);

                        static bool TryReadSendingPipeHandle(ref ReadOnlySequence<byte> buffer)
                        {
                            var delimiterPosition = buffer.PositionOf(NextVersionDaemonProtocolDefaults.Delimiter);

                            if (delimiterPosition == null) {
                                //emptySequence = ReadOnlySequence<byte>.Empty;
                                return false;
                            }

                            //emptySequence = buffer.Slice(0, delimiterPosition.Value);
                            buffer = buffer.Slice(buffer.GetPosition(offset: 1, delimiterPosition.Value));
                            return true;
                        }

                        Exception? capturedError = null;

                        try {
                            while (true) {
                                var result = await reader.ReadAsync();
                                var buffer = result.Buffer;

                                while (TryReadSendingPipeHandle(ref buffer)) {
                                    ResetTimer();
                                    NextVersionResult nextVersionResult;

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
                                }

                                // Tell the PipeReader how much of the buffer has been consumed.
                                reader.AdvanceTo(buffer.Start, buffer.End);

                                // Stop reading if there's no more data coming.
                                if (result.IsCompleted) {
                                    break;
                                }
                            }

                            // Mark the PipeReader as complete.
                            await reader.CompleteAsync();
                        } catch (Exception error) {
                            capturedError = error;
                        }

                        await reader.CompleteAsync(capturedError);
                    }

                    await Task.WhenAll(
                        WriteToPipeAsync(pipe.Writer),
                        ReadFromPipeAsync(pipe.Reader));
                });
            });
    }
}
