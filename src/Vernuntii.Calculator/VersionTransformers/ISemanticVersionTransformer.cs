namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Implements the ability to calculate the next version.
    /// </summary>
    public interface ISemanticVersionTransformer
    {
        /// <summary>
        /// Indicates that the transformer is actually not transforming.
        /// </summary>
        bool DoesNotTransform => false;

        /// <summary>
        /// Transform the version.
        /// </summary>
        /// <param name="version"></param>
        /// <returns>The incremented version number</returns>
        SemanticVersion TransformVersion(SemanticVersion version);
    }
}
