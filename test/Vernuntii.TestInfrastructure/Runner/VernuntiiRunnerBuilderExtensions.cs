using Vernuntii.Git;
using Vernuntii.Git.Commands;
using Vernuntii.Plugins;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem.Reactive;
using Vernuntii.Reactive;

namespace Vernuntii.Runner;

internal static class VernuntiiRunnerBuilderExtensions
{
    public static IVernuntiiRunnerBuilder UseTemporaryRepository(this IVernuntiiRunnerBuilder builder, TemporaryRepository temporaryRepository)
    {
        builder.ConfigurePlugins(plugins => {
            plugins.Add(PluginAction.HandleEvents(events => events
                .Every(GitEvents.OnCustomizeGitCommandCreation)
                .Subscribe(request => request.GitCommandFactory = new GitCommandProvider(temporaryRepository.GitCommand))));
        });

        return builder;
    }
}
