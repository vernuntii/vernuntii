using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    internal class VersionIncrementationServicesView : IVersionIncrementationServicesView
    {
        public IServiceCollection Services { get; }

        public VersionIncrementationServicesView(IServiceCollection services) =>
            Services = services;
    }
}
