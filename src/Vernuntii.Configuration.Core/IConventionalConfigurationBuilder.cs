using Microsoft.Extensions.Configuration;

namespace Vernuntii.Configuration
{
    /// <summary>
    /// Represents a configuration builder with conventional aspects.
    /// </summary>
    public interface IConventionalConfigurationBuilder : IConfigurationBuilder
    {
        /// <summary>
        /// File finder.
        /// </summary>
        IFileFinder FileFinder { get; set; }

        /// <summary>
        /// File finders that follows conventions.
        /// </summary>
        IList<IConventionalFileFinder> ConventionalFileFinders { get; }

        /// <summary>
        /// Adds a new configuration source.
        /// </summary>
        /// <param name="source"></param>
        new IConventionalConfigurationBuilder Add(IConfigurationSource source);

        /// <summary>
        /// Adds file finder to <see cref="ConventionalFileFinders"/>.
        /// </summary>
        /// <param name="fileFinder"></param>
        IConventionalConfigurationBuilder AddConventionalFileFinder(IConventionalFileFinder fileFinder);
    }
}
