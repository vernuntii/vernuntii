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
        /// Event is happening when the git command is requested.
        /// </summary>
        public static readonly EventDiscriminator<GitCommandCreationCustomization> OnCustomizeGitCommandCreation = EventDiscriminator.New<GitCommandCreationCustomization>();

        /// <summary>
        /// Event is happening when the git command has been created.
        /// </summary>
        public static readonly EventDiscriminator CreateGitCommand = EventDiscriminator.New();

        /// <summary>
        /// Event is happening when the git command has been created.
        /// </summary>
        public static readonly EventDiscriminator<IGitCommand> OnCreatedGitCommand = EventDiscriminator.New<IGitCommand>();

        /// <summary>
        /// Event when global service collection is about to be configured.
        /// </summary>
        public static readonly EventDiscriminator<IServiceCollection> OnConfigureServices = EventDiscriminator.New<IServiceCollection>();

        /// <summary>
        /// Event when global service collection has been configured.
        /// </summary>
        public static readonly EventDiscriminator<IServiceCollection> OnConfiguredServices = EventDiscriminator.New<IServiceCollection>();
    }
}
