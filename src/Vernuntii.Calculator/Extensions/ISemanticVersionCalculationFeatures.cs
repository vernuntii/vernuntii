using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Configures the extensions of <see cref="SemanticVersionCalculation"/>.
    /// </summary>
    public interface ISemanticVersionCalculationFeatures
    {
        /// <summary>
        /// The initial services.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
