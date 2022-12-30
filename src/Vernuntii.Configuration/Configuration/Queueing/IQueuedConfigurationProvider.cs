using Microsoft.Extensions.Configuration;

namespace Vernuntii.Configuration.Queueing
{
    /// <summary>
    /// Represents an shadowed configuration provider.
    /// </summary>
    public interface IQueuedConfigurationProvider : IConfigurationProvider
    {
        /// <summary>
        /// The root configuration provider.
        /// </summary>
        IConfigurationProvider RootConfigurationProvider { get; }
    }
}
