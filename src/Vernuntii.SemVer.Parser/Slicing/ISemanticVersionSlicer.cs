namespace Vernuntii.SemVer.Parser.Slicing
{
    /// <summary>
    /// Represents the ability to slice a SemVer version string.
    /// </summary>
    public interface ISemanticVersionSlicer
    {
        /// <summary>
        /// Tries to slice a SemVer-compatible version string into its pieces.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="prefix"></param>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="patch"></param>
        /// <param name="preRelease"></param>
        /// <param name="build"></param>
        bool TrySlice(string value, out string? prefix, out string? major, out string? minor, out string? patch, out string? preRelease, out string? build);
    }
}
