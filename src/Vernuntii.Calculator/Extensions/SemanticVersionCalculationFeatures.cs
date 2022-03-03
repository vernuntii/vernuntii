using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Extensions;

namespace Vernuntii
{
    internal class SemanticVersionCalculationFeatures : ISemanticVersionCalculationFeatures
    {
        public IServiceCollection Services { get; }

        public SemanticVersionCalculationFeatures(IServiceCollection services) => 
            Services = services;
    }
}
