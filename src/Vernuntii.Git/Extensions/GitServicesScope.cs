using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    internal class GitServicesScope : IGitServicesScope
    {
        public IServiceCollection Services { get; }

        public GitServicesScope(IServiceCollection services) =>
            Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
