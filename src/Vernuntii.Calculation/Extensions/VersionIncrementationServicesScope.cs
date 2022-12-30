using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    internal class VersionIncrementationServicesScope : IVersionIncrementationServicesScope
    {
        public IServiceCollection Services { get; }

        public VersionIncrementationServicesScope(IServiceCollection services) =>
            Services = services;
    }
}
