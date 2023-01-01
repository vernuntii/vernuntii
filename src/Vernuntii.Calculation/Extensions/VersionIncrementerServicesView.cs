using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    internal class VersionIncrementerServicesView : IVersionIncrementerServicesView
    {
        public IServiceCollection Services { get; }

        public VersionIncrementerServicesView(IServiceCollection services) =>
            Services = services;
    }
}
