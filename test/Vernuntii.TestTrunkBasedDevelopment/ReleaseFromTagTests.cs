using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Console;
using Vernuntii.Git;
using Vernuntii.Plugins;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.SemVer;
using Xunit;
using Xunit.Priority;

namespace Vernuntii
{
    public class ReleaseFromTagTests : IDisposable
    {
        private readonly TemporaryRepository _temporaryRepository = new(DefaultTemporaryRepositoryLogger);
        private readonly VernuntiiRunner _vernuntii;
        private readonly ConfigureServicesPlugin<IServiceCollection> _configurableCalculationServices = ConfigureServicesPlugin.FromEvent(GitEvents.ConfiguredCalculationServices);

        public ReleaseFromTagTests()
        {
            _vernuntii = new VernuntiiRunnerBuilder()
                .ConfigurePlugins(plugins => {
                    plugins.Add(PluginAction.WhenExecuting<IGitPlugin>.CreatePluginDescriptor(plugin => plugin.SetAlternativeRepository(
                        _temporaryRepository,
                        _temporaryRepository.GitCommand)));

                    plugins.Add(PluginDescriptor.Create(_configurableCalculationServices));
                })
                .Build(new[] {
                    "--config-path",
                    "ReleaseFromTag.yml",
                });
        }

        [Fact, Priority(0)]
        public async Task ReleaseImplicitSnapshot()
        {
            _temporaryRepository.CommitEmpty();

            VersionCaching.IVersionCache versionCache = await _vernuntii.RunAsync();
            _temporaryRepository.TagLightweight(versionCache.Version.Format(SemanticVersionFormat.VersionReleaseBuild));

            ICommitVersion snapshotVersion = _temporaryRepository
                .GetCommitVersions(unsetCache: true)
                .Single();

            Assert.Equal("0.1.0-SNAPSHOT.0", snapshotVersion.Format(SemanticVersionFormat.VersionReleaseBuild));
        }

        [Fact, Priority(1)]
        public async Task ReleaseExplicitFromTag()
        {
            _temporaryRepository.CommitEmpty();
            string releaseVersion = "0.1.0";
            _temporaryRepository.TagLightweight(releaseVersion);
            VersionCaching.IVersionCache versionCache = await _vernuntii.RunAsync();
            Assert.Equal(releaseVersion, versionCache.Version.Format(SemanticVersionFormat.VersionReleaseBuild));
        }

        public void Dispose() => _temporaryRepository.Dispose();
    }
}
