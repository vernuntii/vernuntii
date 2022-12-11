using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions.BranchCases
{
    internal class BranchCasesFeatures : IBranchCasesServicesScope
    {
        public IServiceCollection Services { get; }

        public BranchCasesFeatures(IServiceCollection services) =>
            Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
