using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Provides features to branch cases.
    /// </summary>
    public interface IBranchCasesServicesScope
    {
        /// <summary>
        /// The services.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
