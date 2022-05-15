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
        private TemporaryRepository _temporaryRepository = new TemporaryRepository(DefaultTemporaryRepositoryLogger);
        private VernuntiiRunner _vernuntii;
        private ConfigureServicesPlugin<IServiceCollection> _configurableCalculationServices = ConfigureServicesPlugin.FromEvent(GitEvents.ConfiguredCalculationServices);

        public ReleaseFromTagTests()
        {
            _vernuntii = new VernuntiiRunner() {
                ConsoleArgs = new[] {
                    "--config-path",
                    "ReleaseFromTag.yml"
                },
                PluginDescriptors = new[] {
                    PluginDescriptor.Create(new AlternativeRepositoryPlugin(_temporaryRepository)),
                    PluginDescriptor.Create(_configurableCalculationServices)
                }
            };
        }

        [Fact, Priority(0)]
        public async Task ReleaseImplicitSnapshot()
        {
            _temporaryRepository.CommitEmpty();

            var versionFoundation = await _vernuntii.RunAsync();
            _temporaryRepository.TagLightweight(versionFoundation.Version.Format(SemanticVersionFormat.VersionReleaseBuild));

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
            var versionFoundation = await _vernuntii.RunAsync();
            Assert.Equal(releaseVersion, versionFoundation.Version.Format(SemanticVersionFormat.VersionReleaseBuild));
        }

        public void Dispose() => _temporaryRepository.Dispose();
    }
}
