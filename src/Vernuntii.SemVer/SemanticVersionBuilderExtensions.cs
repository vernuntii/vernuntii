namespace Vernuntii.SemVer
{
    /// <summary>
    /// Extension methods for <see cref="SemanticVersionBuilder"/>.
    /// </summary>
    public static class SemanticVersionBuilderExtensions
    {
        /// <summary>
        /// With major, minor and patch.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="patch"></param>
        public static SemanticVersionBuilder Version(this SemanticVersionBuilder builder, uint major, uint minor, uint patch) => builder
            .Major(major)
            .Minor(minor)
            .Patch(patch);
    }
}
