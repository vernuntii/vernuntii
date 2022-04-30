using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Method extensions for <see cref="IServiceProvider"/>
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Gets the instance of type <see cref="IBranchCase"/> for the active branch from the required
        /// service of type <see cref="IBranchCasesProvider"/> from <paramref name="serviceProvider"/>.
        /// </summary>
        public static IBranchCase GetActiveBranchCase(this IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService<IBranchCasesProvider>().ActiveBranchCase;

        /// <summary>
        /// Gets the default instance of type <see cref="IBranchCase"/> from the required service
        /// of type <see cref="IBranchCasesProvider"/> from <paramref name="serviceProvider"/>.
        /// </summary>
        public static IBranchCase GetDefaultBranchCase(this IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService<IBranchCasesProvider>().DefaultBranchCase;

        /// <summary>
        /// The nested branch cases without default from <see cref="IBranchCasesProvider"/>.
        /// </summary>
        public static IReadOnlyDictionary<string, IBranchCase> GetNestedBranchCases(this IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService<IBranchCasesProvider>().NestedBranchCases;

        /// <summary>
        /// The branch cases with default from <see cref="IBranchCasesProvider"/>.
        /// </summary>
        public static IReadOnlyDictionary<string, IBranchCase> GetBranchCases(this IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService<IBranchCasesProvider>().BranchCases;
    }
}
