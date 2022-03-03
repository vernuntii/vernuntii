using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    internal class SemanticVersionCalculatorFeatures : ISemanticVersionCalculatorFeatures
    {
        public IServiceCollection Services { get; }

        public SemanticVersionCalculatorFeatures(IServiceCollection services) =>
            Services = services;
    }
}
