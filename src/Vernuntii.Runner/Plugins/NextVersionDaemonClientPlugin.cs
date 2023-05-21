using System.CommandLine;
using System.IO.Pipes;
using System.Text;
using Microsoft.Extensions.Logging;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins;

internal class NextVersionDaemonClientPlugin : Plugin
{
    private readonly Option<bool> _daemonClientOption = new Option<bool>("--daemon-client", $"Allows to start {nameof(Vernuntii)} as client.") {
        IsHidden = true
    };

    public readonly INextVersionPlugin _nextVersionPlugin;
    private readonly NextVersionDaemonPlugin _nextVersionDaemonPlugin;
    private readonly ILogger<NextVersionDaemonClientPlugin> _logger;

    public NextVersionDaemonClientPlugin(INextVersionPlugin nextVersionPlugin, NextVersionDaemonPlugin nextVersionDaemonPlugin, ILogger<NextVersionDaemonClientPlugin> logger)
    {
        _nextVersionPlugin = nextVersionPlugin;
        _nextVersionDaemonPlugin = nextVersionDaemonPlugin;
        _logger = logger;
        _nextVersionPlugin.Command.Add(_daemonClientOption);
    }

    protected override void OnExecution()
    {
        var isDaemonClient = false;
        var sendingPipeName = "vernuntii-daemon";

        Events.Once(CommandLineEvents.ParsedCommandLineArguments)
            .Subscribe(parseResult => {
                isDaemonClient = parseResult.GetValueForOption(_daemonClientOption);

                if (isDaemonClient) {
                    _nextVersionDaemonPlugin.AllowEditingPipeHandles = true;
                }
            });


        Events.Once(NextVersionDaemonEvents.OnEditPipes)
            .When(() => isDaemonClient)
            .Subscribe(pipeHandles => {
                //sendingPipe = new NamedPipeClientStream(NextVersionDaemonProtocolDefaults.ServerName, sendingPipeName, PipeDirection.Out);
                //AddDisposable(sendingPipe);
                pipeHandles.DaemonPipeServerName = sendingPipeName;
            });

        Events.Once(LifecycleEvents.BeforeEveryRun)
            .Zip(LifecycleEvents.EndOfRun)
            .When(() => isDaemonClient)
            .Subscribe(result => {
                var (lifecycleContext, _) = result;

                lifecycleContext.NewBackgroundTask(async () => {
                    var daemonClientPipeServerName = "vernuntii-daemon-client" + Guid.NewGuid().ToString();
                    var daemonClientPipeServer = new NamedPipeServerStream(daemonClientPipeServerName, PipeDirection.In, maxNumberOfServerInstances: 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    var nextVersionPipeReader = NextVersionPipeReader.Create(daemonClientPipeServer);

                    while (true) {
                        _ = Console.ReadKey();

                        try {
                            using var sendingPipe = new NamedPipeClientStream(NextVersionDaemonProtocolDefaults.ServerName, sendingPipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                            await sendingPipe.ConnectAsync();

                            sendingPipe.Write(Encoding.ASCII.GetBytes(daemonClientPipeServerName));
                            sendingPipe.WriteByte(NextVersionDaemonProtocolDefaults.Delimiter);
                            sendingPipe.Flush();

                            await daemonClientPipeServer.WaitForConnectionAsync();
                            using var nextVersionMessage = new MemoryStream();
                            var nextVersionMessageType = await nextVersionPipeReader.ReadNextVersionAsync(nextVersionMessage).ConfigureAwait(false);
                            nextVersionPipeReader.ValidateNextVersion(nextVersionMessageType, nextVersionMessage.GetBuffer);
                            Console.WriteLine(Encoding.UTF8.GetString(nextVersionMessage.GetBuffer()));
                        } catch (Exception error) {
                            _logger.LogError(error, "An error occured inside the daemon client");
                        } finally {
                            daemonClientPipeServer.Disconnect();
                        }
                    }
                });
            });
    }
}
