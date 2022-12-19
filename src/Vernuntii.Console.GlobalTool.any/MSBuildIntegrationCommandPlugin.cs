using System.CommandLine;
using Vernuntii.Plugins;
using Vernuntii.PluginSystem;

namespace Vernuntii.Console.GlobalTool
{
    internal class MSBuildIntegrationCommandPlugin : Plugin
    {
        public Command Command { get; }

        private readonly ICommandLinePlugin _commandLine;

        public MSBuildIntegrationCommandPlugin(ICommandLinePlugin commandLine)
        {
            Command = new Command("msbuild-integration");
            _commandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
        }

        protected override void OnExecution() =>
            _commandLine.RootCommand.Add(Command);
    }
}
