using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Configures features of <see cref="SemanticVersionCalculator"/>.
    /// </summary>
    public interface ISemanticVersionCalculatorFeatures
    {
        /// <summary>
        /// The initial services.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
