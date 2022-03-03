namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Default implementation for incrementing the minor version by one
    /// and resetting patch version to zero.
    /// </summary>
    public sealed class NextMinorVersionTransformer : ISemanticVersionTransformer
    {
        /// <summary>
        /// Default instance.
        /// </summary>
        public readonly static NextMinorVersionTransformer Default = new NextMinorVersionTransformer();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="version"></param>
        public SemanticVersion TransformVersion(SemanticVersion version) => version.With
            .Minor(version.Minor + 1)
            .Patch(0);
    }
}
