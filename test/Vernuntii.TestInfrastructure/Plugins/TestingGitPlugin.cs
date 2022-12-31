using Microsoft.Extensions.Logging;

namespace Vernuntii.Plugins;

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
