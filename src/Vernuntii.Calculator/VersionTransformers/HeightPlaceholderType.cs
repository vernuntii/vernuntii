namespace Vernuntii.VersionTransformers
{
    [Flags]
    internal enum HeightPlaceholderType
    {
        Empty = 0,
        Resizing = 1,
        Identifiers = 2 | Resizing,
        IdentifierIndex = 4 | Resizing,
        Height = 8
    }
}
