namespace Vernuntii.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IVernuntiiRunnerBuilder"/>.
    /// </summary>
    public static class VernuntiiRunnerBuilderExtensions
    {
        /// <summary>
        /// Builds the runner for <see cref="Vernuntii"/>.
        /// /// </summary>
        public static VernuntiiRunner Build(this IVernuntiiRunnerBuilder builder) =>
            builder.Build(args: null);
    }
}
