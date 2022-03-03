using Microsoft.Extensions.Options;
using Vernuntii.Git;
using Vernuntii.VersionTransformers;

namespace Vernuntii.Extensions.Configurers
{
    internal class PreReleaseConfigurer : GitConfigurer, IPreReleaseConfigurer, IConfigureOptions<CommitVersionFindingOptions>, IConfigureOptions<SemanticVersionCalculationOptions>
    {
        private bool _handleSearchPreReleaseIdentifier;
        private string? _searchPreReleaseIdentifier;
        private bool _handlePostPreReleaseIdentifier;
        private string? _postPreReleaseIdentifier;

        public PreReleaseConfigurer(IServiceProvider serviceProvider, IRepository repository)
            : base(serviceProvider, repository)
        {
        }

        public void SetSearchPreRelease(string? preRelease)
        {
            _handleSearchPreReleaseIdentifier = true;
            _searchPreReleaseIdentifier = preRelease;
        }

        public void SetPostPreRelease(string? preRelease)
        {
            _handlePostPreReleaseIdentifier = true;
            _postPreReleaseIdentifier = preRelease;
        }

        public void Configure(CommitVersionFindingOptions options)
        {
            if (_handleSearchPreReleaseIdentifier) {
                options.PreRelease = _searchPreReleaseIdentifier;
            }
        }

        public void Configure(SemanticVersionCalculationOptions options)
        {
            if (_handlePostPreReleaseIdentifier) {
                options.PostTransformer = new PreReleaseTransformer(_postPreReleaseIdentifier);
            }
        }
    }
}
