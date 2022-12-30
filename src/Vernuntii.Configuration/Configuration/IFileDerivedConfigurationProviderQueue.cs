using Vernuntii.Configuration.Queueing;

namespace Vernuntii.Configuration
{
    /// <summary>
    /// Provides the configuration of <see cref="IConfigurationProviderQueue"/> with additional informations.
    /// </summary>
    public interface IFileDerivedConfigurationProviderQueue : IConfigurationProviderQueue
    {
        /// <summary>
        /// Represents the file that you are about to add via <see cref="IConfigurationProviderQueue.RootConfigurationProvider"/>.
        /// </summary>
        FileInfo FileInfo { get; }
    }
}
