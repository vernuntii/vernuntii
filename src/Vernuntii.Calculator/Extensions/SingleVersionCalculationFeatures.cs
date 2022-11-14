using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Extensions;

namespace Vernuntii
{
    internal class SingleVersionCalculationFeatures : ISingleVersionCalculationFeatures
    {
        public IServiceCollection Services { get; }

        public SingleVersionCalculationFeatures(IServiceCollection services) =>
            Services = services;
    }
}
