using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Extensions;

namespace Vernuntii
{
    internal class VersionIncrementationServicesScope : IVersionIncrementationServicesScope
    {
        public IServiceCollection Services { get; }

        public VersionIncrementationServicesScope(IServiceCollection services) =>
            Services = services;
    }
}
