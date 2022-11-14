using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Git.Commands;
using Vernuntii.PluginSystem.Events;

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
        public static readonly SubjectEvent<string> ResolvedGitWorkingTreeDirectory = new();

        /// <summary>
        /// Event is happening when the git command has been created.
        /// </summary>
        public static readonly SubjectEvent<IGitCommand> CreatedGitCommand = new();

        /// <summary>
        /// Event when global service collection is about to be configured.
        /// </summary>
        public static readonly SubjectEvent<IServiceCollection> ConfiguringGlobalServices = new();

        /// <summary>
        /// Event when global service collection has been configured.
        /// </summary>
        public static readonly SubjectEvent<IServiceCollection> ConfiguredGlobalServices = new();

        /// <summary>
        /// Event when calculation service collection is about to be configured.
        /// </summary>
        public static readonly SubjectEvent<IServiceCollection> ConfiguringCalculationServices = new();

        /// <summary>
        /// Event when calculation service collection has been configured.
        /// </summary>
        public static readonly SubjectEvent<IServiceCollection> ConfiguredCalculationServices = new();
    }
}
