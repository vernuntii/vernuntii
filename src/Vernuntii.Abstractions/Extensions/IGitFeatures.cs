using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Represents the configurable extensions of the assemblys.
    /// </summary>
    public interface IGitFeatures
    {
        /// <summary>
        /// The initial services.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
