namespace Vernuntii.Git
{
    internal record CloneOptions
    {
        private string _sourceUrl { get; }
        public CloneSource Source { get; init; } = CloneSource.Remote;
        public int? Depth { get; init; }

        public CloneOptions(string sourceUrl) =>
            _sourceUrl = sourceUrl ?? throw new ArgumentNullException(nameof(sourceUrl));

        internal string SourceUrl() => Source switch {
            CloneSource.Remote => _sourceUrl,
            CloneSource.File => $"file://{_sourceUrl}",
            _ => throw new InvalidOperationException("Bad clone source")
        };
    }
}
