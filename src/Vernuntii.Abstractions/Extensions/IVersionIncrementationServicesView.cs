using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Configures the extensions of a version incrementation.
    /// </summary>
    public interface IVersionIncrementationServicesView
    {
        /// <summary>
        /// The initial services.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}

