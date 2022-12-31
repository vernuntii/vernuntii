using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Git;
using Vernuntii.Git.Commands;
using Vernuntii.Plugins;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.Runner;
using Vernuntii.SemVer;
using Xunit;
using Xunit.Priority;

namespace Vernuntii
{
    public class ReleaseFromTagTests : IDisposable
    {
        private readonly TemporaryRepository _temporaryRepository = new(DefaultTemporaryRepositoryLogger);
        private readonly ConfigureServicesPlugin<IServiceCollection> _configurableServices = ConfigureServicesPlugin.FromEvent(GitEvents.ConfiguredServices);
        private readonly VernuntiiRunner _vernuntii;

        public ReleaseFromTagTests()
        {
            _vernuntii = VernuntiiRunnerBuilder.WithNextVersionRequirements()
                .ConfigurePlugins(plugins => {
                    plugins.Add(
                        PluginEventAction.OnEveryEvent(
                            GitEvents.RequestGitCommandFactory,
                            request => request.GitCommandFactory = new GitCommandProvider(_temporaryRepository.GitCommand)));

                    plugins.Add(_configurableServices);
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

            var nextVersion = await _vernuntii.NextVersionAsync();
            _temporaryRepository.TagLightweight(nextVersion.Format(SemanticVersionFormat.VersionReleaseBuild));

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
            var nextVersion = await _vernuntii.NextVersionAsync();
            Assert.Equal(releaseVersion, nextVersion.Format(SemanticVersionFormat.VersionReleaseBuild));
        }

        public void Dispose() => _temporaryRepository.Dispose();
    }
}
