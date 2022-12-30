using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    internal class VersionIncrementerServicesScope : IVersionIncrementerServicesScope
    {
        public IServiceCollection Services { get; }

        public VersionIncrementerServicesScope(IServiceCollection services) =>
            Services = services;
    }
}
