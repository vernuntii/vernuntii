namespace Vernuntii.MessageVersioning
{
    /// <summary>
    /// Represents the context for <see cref="VersionTransformerBuilder"/>.
    /// </summary>
    public record MessageVersioningContext
    {
        /// <summary>
        /// The options of on-going calculation.
        /// </summary>
        public SemanticVersionCalculationOptions CalculationOptions { get; }

        /// <summary>
        /// The current version before the next transformation is applied.
        /// </summary>
        public SemanticVersion CurrentVersion {
            get => _currentVersion ?? CalculationOptions.StartVersion;
            init => _currentVersion = value;
        }

        private SemanticVersion? _currentVersion;

        /// <summary>
        /// Creates in instance of <see cref="MessageVersioningContext"/>
        /// </summary>
        /// <param name="calculationOptions"></param>
        public MessageVersioningContext(SemanticVersionCalculationOptions calculationOptions) =>
            CalculationOptions = calculationOptions;
    }
}
