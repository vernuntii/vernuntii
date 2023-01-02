using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions.BranchCases
{
    internal class BranchCasesServicesView : IBranchCasesServicesView
    {
        public IServiceCollection Services { get; }

        public BranchCasesServicesView(IServiceCollection services) =>
            Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
