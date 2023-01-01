using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions.BranchCases
{
    internal class BranchCasesFeatures : IBranchCasesServicesView
    {
        public IServiceCollection Services { get; }

        public BranchCasesFeatures(IServiceCollection services) =>
            Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
