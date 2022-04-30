namespace Vernuntii.HeightConventions.Transformation
{
    /// <summary>
    /// Parses a placeholder (e.g. a part of an dotted identifier) what type it is.
    /// </summary>
    public interface IHeightPlaceholderParser
    {
        /// <summary>
        /// Parses a placeholder.
        /// </summary>
        /// <param name="placeholder"></param>
        /// <param name="placeholderContent"></param>
        /// <returns>The type of placeholder.</returns>
        HeightPlaceholderType Parse(ReadOnlySpan<char> placeholder, out object? placeholderContent);
    }
}
