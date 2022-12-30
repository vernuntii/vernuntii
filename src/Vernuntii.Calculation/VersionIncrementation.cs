using Vernuntii.SemVer;

namespace Vernuntii
{
    /// <summary>
    /// Represents a pre-configured incrementation of a version.
    /// </summary>
    internal class VersionIncrementation : IVersionIncrementation
    {
        private readonly IVersionIncrementer _incrementer;
        private readonly VersionIncrementationOptions _incrementationOptions;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="incrementer"></param>
        /// <param name="incrmentationOptions"></param>
        public VersionIncrementation(IVersionIncrementer incrementer, VersionIncrementationOptions incrmentationOptions)
        {
            _incrementer = incrementer;
            _incrementationOptions = incrmentationOptions;
        }

        /// <inheritdoc/>
        public ISemanticVersion GetIncrementedVersion() => _incrementer.IncrementVersion(_incrementationOptions);
    }
}
