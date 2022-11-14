using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vernuntii.Plugins;
using Vernuntii.PluginSystem;

namespace Vernuntii.Console.GlobalTool
{
    internal class MSBuildIntegrationCommandPlugin : Plugin
    {
        public Command Command => command;

        private readonly ICommandLinePlugin _commandLine;
        private Command command;

        public MSBuildIntegrationCommandPlugin(ICommandLinePlugin commandLine)
        {
            command = new Command("msbuild-integration");
            _commandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
        }

        protected override void OnExecution() =>
            _commandLine.RootCommand.Add(command);
    }
}
