namespace Vernuntii.VersionTransformers
{
    internal interface IHeightPlaceholderParser
    {
        object? ParsePlaceholder(ReadOnlySpan<char> placeholder, out HeightPlaceholderType placeholderType);
    }
}
