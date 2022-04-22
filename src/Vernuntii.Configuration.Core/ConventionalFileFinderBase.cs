using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration.Shadowing;

namespace Vernuntii.Configuration
{
    /// <summary>
    /// Abstract class that implements <see cref="IConventionalConfigurationBuilder"/>.
    /// </summary>
    public abstract class ConventionalFileFinderBase : IConventionalFileFinder
    {
        /// <summary>
        /// The file extension this file finder is able to find.
        /// </summary>
        protected abstract string[] ProbeableFileExtensions { get; }

        /// <inheritdoc/>
        public virtual bool IsProbeable(string? fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            return fileName != null && Array.Exists(ProbeableFileExtensions, x => string.Equals(x, fileExtension, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public abstract IFileFindingEnumerator FindFile(string directoryPath, string fileName);

        /// <inheritdoc/>
        public abstract void AddFile(IConfigurationBuilder builder, string filePath, Action<IShadowedConfigurationProviderBuilderConfigurator>? configureProviderBuilder = null);
    }
}
