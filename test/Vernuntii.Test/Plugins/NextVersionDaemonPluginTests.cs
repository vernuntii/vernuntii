using System.IO.Pipes;
using System.Text;
using FluentAssertions;
using Vernuntii.Extensions;
using Vernuntii.Git;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem.Reactive;
using Vernuntii.Reactive;
using Vernuntii.Runner;
using Vernuntii.SemVer;
using Vernuntii.VersioningPresets;
using Vernuntii.VersionTransformers;

namespace Vernuntii.Plugins
{
    public class NextVersionDaemonPluginTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public async Task NextVersionDaemon_should_generate_version_and_adapt_next_version(int calculateNextXSuccessiveReleaseVersions)
        {
            // Arrange
            using var repository = new TemporaryRepository();
            repository.CommitEmpty();

            await using var sendingPipe = new AnonymousPipeServerStream(PipeDirection.Out);
            await using var receivingPipe = new AnonymousPipeServerStream(PipeDirection.In);
            var receivingPipeReader = NextVersionPipeReader.Create(receivingPipe);

            await using var runner = new VernuntiiRunnerBuilder()
                .UseTemporaryRepository(repository)
                .Build(args: new[] {
                    "--daemon",
                    sendingPipe.GetClientHandleAsString(),
                    receivingPipe.GetClientHandleAsString()
                });

            using var overrideVersioningMode = runner.PluginEvents.Once(ServicesEvents.OnConfigureServices)
                .Subscribe(services => services
                    .TakeViewOfVernuntii()
                    .AddVersionIncrementation(incrementation => incrementation.PostConfigure(options => {
                        options.PostTransformer = new PreReleaseTransformer(preRelease: null);
                        options.VersioningPreset = VersioningPreset.ContinuousDeployment;
                    })));

            List<ISemanticVersion> expectedNextVersions = new();

            using var nextVersionSubscription = runner.PluginEvents.Every(NextVersionEvents.OnCalculatedNextVersion)
                .Subscribe(nextVersionResult => expectedNextVersions.Add(nextVersionResult.VersionCache.Version));

            // Act
            var runTask = runner.RunAsync();
            var actualNextVersionStrings = new List<string>();

            for (var i = 0; i < calculateNextXSuccessiveReleaseVersions; i++) {
                sendingPipe.WriteByte(NextVersionDaemonProtocolDefaults.Delimiter);
                await sendingPipe.FlushAsync();

                using var nextVersionBytes = new MemoryStream();
                var responseType = await receivingPipeReader.ReadNextVersionAsync(nextVersionBytes);
                receivingPipeReader.ValidateNextVersion(responseType, nextVersionBytes.GetBuffer);
                nextVersionBytes.Position = 0;

                var nextVersion = Encoding.UTF8.GetString(nextVersionBytes.GetBuffer(), 0, (int)nextVersionBytes.Length);
                actualNextVersionStrings.Add(nextVersion);

                if (i + 1 < calculateNextXSuccessiveReleaseVersions) {
                    // Add another commit to get incremented version next run
                    repository.CommitEmpty();
                }
            }

            await sendingPipe.DisposeAsync();
            await runTask;

            // Assert
            expectedNextVersions.Should().NotBeEmpty();
            actualNextVersionStrings.Should().NotBeEmpty();

            var actualNextVersions = actualNextVersionStrings.Select(SemanticVersion.Parse).ToList();
            Assert.Equal(expectedNextVersions, actualNextVersions, SemanticVersionComparer.VersionReleaseBuild);

            // Let's ensure every next version is incremented
            _ = actualNextVersions.Aggregate((previous, current) => {
                previous.Patch.Should().BeLessThan(current.Patch);
                return current;
            });
        }
    }
}
