namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Default implementation for incrementing the patch version by one.
    /// </summary>
    public sealed class NextPatchVersionTransformer : ISemanticVersionTransformer
    {
        /// <summary>
        /// Default instance.
        /// </summary>
        public readonly static NextPatchVersionTransformer Default = new NextPatchVersionTransformer();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="version"></param>
        public SemanticVersion TransformVersion(SemanticVersion version) =>
            version.With.Patch(version.Patch + 1);
    }
}
