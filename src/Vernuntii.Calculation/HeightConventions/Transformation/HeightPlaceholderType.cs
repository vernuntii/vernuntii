namespace Vernuntii.HeightConventions.Transformation
{
    /// <summary>
    /// The type of placeholder.
    /// </summary>
    [Flags]
    public enum HeightPlaceholderType
    {
        /// <summary>
        /// Placeholder for nothing.
        /// </summary>
        Empty = 0,
        /// <summary>
        /// Placeholder for identifiers.
        /// </summary>
        Identifiers = 2,
        /// <summary>
        /// Placeholder for not yet known content of identifier at index of dotted identifier.
        /// </summary>
        IdentifierIndex = 4,
        /// <summary>
        /// Placeholder for height.
        /// </summary>
        Height = 8
    }
}
