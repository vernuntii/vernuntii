using Vernuntii.Git;
using Vernuntii.Git.Commands;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// The git plugin.
    /// Its capabilities is to create a pre-initalized git command
    /// and second to register it to the global service collection.
    /// </summary>
    public interface IGitPlugin : IPlugin
    {
        /// <summary>
        /// The git directory. It is available after
        /// <see cref="ConfigurationEvents.ConfiguredConfigurationBuilder"/>.
        /// </summary>
        string WorkingTreeDirectory { get; }

        /// <summary>
        /// The git command.
        /// </summary>
        IGitCommand GitCommand { get; }

        /// <summary>
        /// Sets an alternative repository.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="gitCommand"></param>
        void SetAlternativeRepository(IRepository repository, IGitCommand gitCommand);

        /// <summary>
        /// Unsets the alternative repository.
        /// </summary>
        void UnsetAlternativeRepository();
    }
}
