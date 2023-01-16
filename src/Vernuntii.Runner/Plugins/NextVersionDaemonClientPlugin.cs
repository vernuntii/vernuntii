using System.CommandLine;
using System.IO.Pipes;
using System.Text;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins;

internal class NextVersionDaemonClientPlugin : Plugin
{
    public readonly INextVersionPlugin _nextVersionPlugin;
    private readonly NextVersionDaemonPlugin _nextVersionDaemonPlugin;

    private readonly Option<bool> _daemonClientOption = new Option<bool>("--daemon-client", $"Allows to start {nameof(Vernuntii)} as client.") {
        IsHidden = true
    };

    public NextVersionDaemonClientPlugin(INextVersionPlugin nextVersionPlugin, NextVersionDaemonPlugin nextVersionDaemonPlugin)
    {
        _nextVersionPlugin = nextVersionPlugin;
        _nextVersionDaemonPlugin = nextVersionDaemonPlugin;
        _nextVersionPlugin.Command.Add(_daemonClientOption);
    }

    protected override void OnExecution()
    {
        var isDaemonClient = false;
        AnonymousPipeServerStream? sendingPipe = null;
        AnonymousPipeServerStream? receivingPipe = null;

        Events.Once(CommandLineEvents.ParsedCommandLineArguments)
            .Subscribe(parseResult => {
                isDaemonClient = parseResult.GetValueForOption(_daemonClientOption);

                if (isDaemonClient) {
                    _nextVersionDaemonPlugin.AllowEditingPipeHandles = true;
                }
            });

        Events.Once(NextVersionDaemonEvents.OnEditPipeHandles)
            .When(() => isDaemonClient)
            .Subscribe(pipeHandles => {
                sendingPipe = new AnonymousPipeServerStream(PipeDirection.Out);
                AddDisposable(sendingPipe);
                pipeHandles.ReceivingPipeHandle = sendingPipe.GetClientHandleAsString();

                receivingPipe = new AnonymousPipeServerStream(PipeDirection.In);
                AddDisposable(receivingPipe);
                pipeHandles.SendingPipeHandle = receivingPipe.GetClientHandleAsString();
            });

        Events.Once(LifecycleEvents.BeforeEveryRun)
            .Zip(LifecycleEvents.EndOfRun)
            .When(() => isDaemonClient)
            .Subscribe(result => {
                var (lifecycleContext, _) = result;

                lifecycleContext.NewBackgroundTask(async () => {
                    if (receivingPipe is null || sendingPipe is null) {
                        throw new InvalidOperationException("The pipe servers have not been created");
                    }

                    var nextVersionPipeReader = NextVersionPipeReader.Create(receivingPipe);

                    while (true) {
                        _ = Console.ReadKey();
                        sendingPipe.WriteByte(NextVersionDaemonProtocolDefaults.Delimiter);
                        sendingPipe.Flush();

                        using var nextVersionMessage = new MemoryStream();
                        var nextVersionMessageType = await nextVersionPipeReader.ReadNextVersionAsync(nextVersionMessage).ConfigureAwait(false);
                        nextVersionPipeReader.ValidateNextVersion(nextVersionMessageType, nextVersionMessage.GetBuffer);
                        Console.WriteLine(Encoding.UTF8.GetString(nextVersionMessage.GetBuffer()));
                    }
                });
            });
    }
}
