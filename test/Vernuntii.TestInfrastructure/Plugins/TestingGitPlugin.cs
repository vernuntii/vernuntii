using Microsoft.Extensions.Logging;
using Vernuntii.Plugins;

namespace Vernuntii.Test.Plugins;

internal class TestingGitPlugin : GitPlugin
{
    public TestingGitPlugin(
        ICommandLinePlugin commandlinePlugin,
        SharedOptionsPlugin sharedOptions,
        INextVersionPlugin nextVersionPlugin,
        ILogger<GitPlugin> logger) : base(
            commandlinePlugin,
            sharedOptions,
            nextVersionPlugin,
            logger)
    {
    }
}
