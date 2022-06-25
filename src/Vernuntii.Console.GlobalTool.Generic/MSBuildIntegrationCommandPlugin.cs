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

        private Command command;

        public MSBuildIntegrationCommandPlugin() =>
            command = new Command("msbuild-integration");

        protected override void OnAfterRegistration() =>
            Plugins.First<ICommandLinePlugin>().RootCommand.Add(command);
    }
}
