using Microsoft.Extensions.Configuration;

namespace Vernuntii.Configuration.Queueing
{
    /// <summary>
    /// Represents the building process of <see cref="IQueuedConfigurationProvider"/>.
    /// </summary>
    public interface IConfigurationProviderQueue
    {
        /// <summary>
        /// The root configuration provider.
        /// </summary>
        IConfigurationProvider RootConfigurationProvider { get; }

        /// <summary>
        /// Adds another <paramref name="configurationProvider"/>.
        /// </summary>
        /// <param name="configurationProvider"></param>
        void Enqueue(IConfigurationProvider configurationProvider);
    }
}
