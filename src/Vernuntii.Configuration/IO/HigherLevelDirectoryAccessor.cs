namespace Vernuntii.IO
{
    /// <summary>
    /// Accessor for upward directory.
    /// </summary>
    public readonly struct HigherLevelDirectoryAccessor
    {
        /// <summary>
        /// Represents the enumerator that enumerates higher-level directories as long as
        /// <see cref="DirectoryInfo"/> is <see langword="null"/>.
        /// </summary>
        public IEnumerator<DirectoryInfo?> Enumerator { get; }

        /// <summary>
        /// Creates an instance of <see cref="HigherLevelDirectoryAccessor"/>.
        /// </summary>
        /// <param name="enumerator"></param>
        public HigherLevelDirectoryAccessor(IEnumerator<DirectoryInfo?> enumerator) =>
            Enumerator = enumerator;

        /// <summary>
        /// Gets the upward directory.
        /// </summary>
        /// <returns>The upward directory or null.</returns>
        public DirectoryInfo? GetUpwardDirectory()
        {
            if (Enumerator is null) {
                goto exit;
            }

            while (Enumerator.MoveNext()) {
                if (Enumerator.Current is not null) {
                    return Enumerator.Current;
                }
            }

            exit:
            return null;
        }
    }
}
