using Vernuntii.MessageConventions;
using Vernuntii.MessageConventions.MessageIndicators;
using Vernuntii.MessagesProviders;
using Vernuntii.SemVer;
using Vernuntii.VersionIncrementFlows;
using Vernuntii.VersionIncrementing;
using Vernuntii.VersionTransformers;
using Xunit;

namespace Vernuntii.VersioningPresets
{
    public class FlowDownstreamWhenZeroMajorTests
    {
        public static IEnumerable<object[]> BuildIncrementShouldTransformVersionsGenerator()
        {
            yield return new object[] {
                SemanticVersion.OnePatch,
                true,
                SemanticVersion.OneMinor
            };

            yield return new object[] {
                SemanticVersion.OneMinor,
                true,
                SemanticVersion.OneMinor.With.Minor(2).ToVersion()
            };

            yield return new object[] {
                SemanticVersion.OneMajor,
                false,
                SemanticVersion.OneMajor.With.Major(2).ToVersion()
            };
        }

        [Theory]
        [MemberData(nameof(BuildIncrementShouldTransformVersionsGenerator))]
        public void BuildIncrementShouldTransformVersions(ISemanticVersion startVersion, bool expectedDownstreamFlow, ISemanticVersion expectedVersion)
        {
            VersioningPreset preset = new() {
                IncrementMode = VersionIncrementMode.Successive,
                MessageConvention = new MessageConvention() {
                    MajorIndicators = new[] { TruthyMessageIndicator.Default }
                },
                IncrementFlow = VersionIncrementFlow.ZeroMajorDownstream,
            };

            VersionIncrementBuilder builder = new();

            VersionIncrementContext context = new(new VersionIncrementationOptions() {
                StartVersion = startVersion,
                VersioningPreset = preset
            });

            var nextVersion = builder.BuildIncrement(new Message(), context).TransformVersion(startVersion);
            Assert.Equal(expectedDownstreamFlow, context.IsVersionDownstreamFlowed);
            Assert.Equal(expectedVersion, nextVersion);
        }
    }
}
