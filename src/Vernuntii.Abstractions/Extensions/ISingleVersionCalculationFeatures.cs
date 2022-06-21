using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Configures the extensions of <see cref="SingleVersionCalculation"/>.
    /// </summary>
    public interface ISingleVersionCalculationFeatures
    {
        /// <summary>
        /// The initial services.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
