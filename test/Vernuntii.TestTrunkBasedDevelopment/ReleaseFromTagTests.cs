using Vernuntii.Git;
using Vernuntii.Runner;
using Vernuntii.SemVer;
using Xunit;
using Xunit.Priority;

namespace Vernuntii
{
    public class ReleaseFromTagTests : IAsyncLifetime
    {
        private readonly TemporaryRepository _temporaryRepository = new();
        //private readonly ConfigureServicesPlugin<IServiceCollection> _configurableServices = ConfigureServicesPlugin.FromEvent(GitEvents.OnConfiguredServices);
        private readonly VernuntiiRunner _vernuntii;

        public ReleaseFromTagTests()
        {
            _vernuntii = new VernuntiiRunnerBuilder()
                .UseTemporaryRepository(_temporaryRepository)
                //.ConfigurePlugins(plugins => {
                //    plugins.Add(_configurableServices);
                //})
                .Build(new[] {
                    "--config-path",
                    "ReleaseFromTag.yml",
                });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        [Fact, Priority(0)]
        public async Task ReleaseImplicitSnapshot()
        {
            _temporaryRepository.CommitEmpty();

            var nextVersionResult = await _vernuntii.NextVersionAsync();
            _temporaryRepository.TagLightweight(nextVersionResult.VersionCache.Version.Format(SemanticVersionFormat.VersionReleaseBuild));

            var snapshotVersion = _temporaryRepository
                .GetCommitVersions(unsetCache: true)
                .Single();

            Assert.Equal("0.1.0-SNAPSHOT.0", snapshotVersion.Format(SemanticVersionFormat.VersionReleaseBuild));
        }

        [Fact, Priority(1)]
        public async Task ReleaseExplicitFromTag()
        {
            _temporaryRepository.CommitEmpty();
            var releaseVersion = "0.1.0";
            _temporaryRepository.TagLightweight(releaseVersion);
            var nextVersionResult = await _vernuntii.NextVersionAsync();
            Assert.Equal(releaseVersion, nextVersionResult.VersionCache.Version.Format(SemanticVersionFormat.VersionReleaseBuild));
        }

        public async Task DisposeAsync()
        {
            await _vernuntii.DisposeAsync();
            _temporaryRepository.Dispose();
        }
    }
}
