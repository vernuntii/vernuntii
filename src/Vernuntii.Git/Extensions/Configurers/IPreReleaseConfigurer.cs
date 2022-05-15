using Vernuntii.Git;
using Vernuntii.VersionTransformers;

namespace Vernuntii.Extensions.Configurers
{
    /// <summary>
    /// Provides configuration capabilities for pre-release.
    /// </summary>
    public interface IPreReleaseConfigurer
    {
        /// <summary>
        /// Service provider.
        /// </summary>
        IServiceProvider ServiceProvider { get; }
        /// <summary>
        /// The repository from service provider.
        /// </summary>
        IRepository Repository { get; }

        /// <summary>
        /// Sets <see cref="CommitVersionFindingOptions.PreRelease"/>.
        /// </summary>
        /// <param name="preRelease"></param>
        void SetSearchPreRelease(string? preRelease);

        /// <summary>
        /// Sets <see cref="SingleVersionCalculationOptions.PostTransformer"/> to
        /// <see cref="PreReleaseTransformer"/> with <paramref name="preRelease"/>.
        /// </summary>
        /// <param name="preRelease"></param>
        void SetPostPreRelease(string? preRelease);
    }
}
