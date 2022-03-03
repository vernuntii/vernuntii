using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    internal class GitFeatures : IGitFeatures
    {
        public IServiceCollection Services { get; }

        public GitFeatures(IServiceCollection services) =>
            Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
