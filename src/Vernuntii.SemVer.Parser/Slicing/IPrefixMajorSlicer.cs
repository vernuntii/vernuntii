namespace Vernuntii.SemVer.Parser.Slicing
{
    /// <summary>
    /// A prefix-major slicer.
    /// </summary>
    public interface IPrefixMajorSlicer
    {
        /// <summary>
        /// Slices a value that is a combination of a prefix and the major version number.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="prefix"></param>
        /// <param name="major"></param>
        public void Slice(ReadOnlySpan<char> value, out string prefix, out string major);
    }
}
