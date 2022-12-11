using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Configures features of a version incrementer.
    /// </summary>
    public interface IVersionIncrementerServicesScope
    {
        /// <summary>
        /// The initial services.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
