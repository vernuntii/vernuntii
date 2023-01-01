using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    internal class GitServicesView : IGitServicesView
    {
        public IServiceCollection Services { get; }

        public GitServicesView(IServiceCollection services) =>
            Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
