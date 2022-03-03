using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Method extensions for <see cref="IServiceProvider"/>
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Gets the instance of type <see cref="IBranchCaseArguments"/> for the active branch from the required
        /// service of type <see cref="IBranchCaseArgumentsProvider"/> from <paramref name="serviceProvider"/>.
        /// </summary>
        public static IBranchCaseArguments GetActiveBranchCase(this IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService<IBranchCaseArgumentsProvider>().ActiveBranchCase;

        /// <summary>
        /// Gets the default instance of type <see cref="IBranchCaseArguments"/> from the required service
        /// of type <see cref="IBranchCaseArgumentsProvider"/> from <paramref name="serviceProvider"/>.
        /// </summary>
        public static IBranchCaseArguments GetDefaultBranchCase(this IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService<IBranchCaseArgumentsProvider>().DefaultBranchCase;

        /// <summary>
        /// The nested branch cases without default from <see cref="IBranchCaseArgumentsProvider"/>.
        /// </summary>
        public static IReadOnlyDictionary<string, IBranchCaseArguments> GetNestedBranchCases(this IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService<IBranchCaseArgumentsProvider>().NestedBranchCases;

        /// <summary>
        /// The branch cases with default from <see cref="IBranchCaseArgumentsProvider"/>.
        /// </summary>
        public static IReadOnlyDictionary<string, IBranchCaseArguments> GetBranchCases(this IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService<IBranchCaseArgumentsProvider>().BranchCases;
    }
}
