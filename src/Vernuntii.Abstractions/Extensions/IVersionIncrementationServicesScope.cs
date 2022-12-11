using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Configures the extensions of a version incrementation.
    /// </summary>
    public interface IVersionIncrementationServicesScope
    {
        /// <summary>
        /// The initial services.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}

