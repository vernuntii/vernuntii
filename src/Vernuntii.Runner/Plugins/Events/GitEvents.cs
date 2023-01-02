using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Git.Commands;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="IGitPlugin"/>.
    /// </summary>
    public static class GitEvents
    {
        /// <summary>
        /// Event when fully qualified git directory has been resolved.
        /// </summary>
        public static readonly EventDiscriminator<string> ResolvedGitWorkingTreeDirectory = EventDiscriminator.New<string>();

        /// <summary>
        /// Event is happening when the git command is requested.
        /// </summary>
        public static readonly EventDiscriminator<GitCommandFactoryRequest> RequestGitCommandFactory = EventDiscriminator.New<GitCommandFactoryRequest>();

        /// <summary>
        /// Event is happening when the git command has been created.
        /// </summary>
        public static readonly EventDiscriminator CreateGitCommand = EventDiscriminator.New();

        /// <summary>
        /// Event is happening when the git command has been created.
        /// </summary>
        public static readonly EventDiscriminator<IGitCommand> CreatedGitCommand = EventDiscriminator.New<IGitCommand>();

        /// <summary>
        /// Event when global service collection is about to be configured.
        /// </summary>
        public static readonly EventDiscriminator<IServiceCollection> ConfigureServices = EventDiscriminator.New<IServiceCollection>();

        /// <summary>
        /// Event when global service collection has been configured.
        /// </summary>
        public static readonly EventDiscriminator<IServiceCollection> ConfiguredServices = EventDiscriminator.New<IServiceCollection>();
    }
}
