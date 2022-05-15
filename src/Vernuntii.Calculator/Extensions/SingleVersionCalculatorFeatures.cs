using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    internal class SingleVersionCalculatorFeatures : ISingleVersionCalculatorFeatures
    {
        public IServiceCollection Services { get; }

        public SingleVersionCalculatorFeatures(IServiceCollection services) =>
            Services = services;
    }
}
