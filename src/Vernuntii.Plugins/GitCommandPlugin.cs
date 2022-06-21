using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Git;
using Vernuntii.Git.Command;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// The git command plugin. Its capabilities is first to create a pre-initalized 
    /// git command and second to register it to the global service collection.
    /// </summary>
    public class GitCommandPlugin : Plugin, IGitCommandPlugin
    {
        /// <summary>
        /// The git command that is available after <see cref="GitEvents.ResolvedGitDirectory"/>.
        /// </summary>
        public IGitCommand GitCommand => _gitCommand ?? throw new InvalidOperationException($"The event \"{nameof(GitEvents.ResolvedGitDirectory)}\" was not yet called");

        private IGitCommand? _gitCommand;

        /// <inheritdoc/>
        protected override void OnEvents()
        {
            Events.SubscribeOnce(GitEvents.ResolvedGitDirectory, gitDirectory => {
                _gitCommand = new GitCommand(gitDirectory);
                Events.Publish(GitCommandEvents.CreatedCommand, _gitCommand);
            });

            Events.SubscribeOnce(GlobalServicesEvents.ConfigureServices, services => services
                .AddOptions<RepositoryOptions>()
                    .Configure(options => options.GitCommandFactory = new GitCommandProvider(GitCommand)));
        }
    }
}
