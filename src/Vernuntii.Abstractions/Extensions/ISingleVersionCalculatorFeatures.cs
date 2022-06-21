using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Configures features of <see cref="SingleVersionCalculator"/>.
    /// </summary>
    public interface ISingleVersionCalculatorFeatures
    {
        /// <summary>
        /// The initial services.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
