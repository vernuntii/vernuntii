using System.CommandLine;
using System.IO.Pipes;
using Nerdbank.Streams;
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
    private const string TwoRequiredAnonymousPipesMessage = "Two anonymous pipe handles are required: the first that can receive and the second that can send.";

    private readonly VernuntiiRunner _runner;
    private readonly INextVersionPlugin _nextVersionPlugin;
    private readonly Option<string[]> _daemonOption = new Option<string[]>("--daemon", $"Allows to start {nameof(Vernuntii)} as daemon." +
        $" {TwoRequiredAnonymousPipesMessage}") {
        Arity = new ArgumentArity(minimumNumberOfValues: 2, maximumNumberOfValues: 2)
    };

    public NextVersionDaemonPlugin(VernuntiiRunner runner, INextVersionPlugin nextVersionPlugin)
    {
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _nextVersionPlugin = nextVersionPlugin ?? throw new ArgumentNullException(nameof(nextVersionPlugin));
        _nextVersionPlugin.Command.Add(_daemonOption);
    }

    protected override void OnExecution()
    {
        //var receivingPipe = default(AnonymousPipeClientStream);
        //var sendingPipe = default(AnonymousPipeClientStream);

        //Events.Earliest(CommandLineEvents.ParsedCommandLineArguments)
        //    .Where(_ => _nextVersionPlugin.Command.IsSeatTaken)
        //    .Subscribe(x => {
        //        static InvalidOperationException CreateException() => 
        //            new InvalidOperationException(TwoRequiredAnonymousPipesMessage);

        //        var pipeHandles = x.GetValueForOption(_daemonOption) ?? throw CreateException();

        //        if (pipeHandles.Length != 2) {
        //            throw CreateException();
        //        }

        //        receivingPipe = new AnonymousPipeClientStream(PipeDirection.In, pipeHandles[0]);
        //        AddDisposable(receivingPipe);

        //        sendingPipe = new AnonymousPipeClientStream(PipeDirection.Out, pipeHandles[1]);
        //        AddDisposable(sendingPipe);
        //    });

        //// We skip the the very first "next version"-invocation.
        //Events.Earliest(NextVersionEvents.OnInvokeNextVersionCommand)
        //    .Subscribe(nextVersionCommandInvocation => {
        //        nextVersionCommandInvocation.IsHandled = true;

        //        _ = Task.Run(async () => { 
        //            receivingPipe.ReadBlockAsync()
        //        });
        //    });

        //Events.Every(NextVersionEvents.OnCalculatedNextVersion)
        //    .Subscribe(versionCache => {

        //    });
    }
}
