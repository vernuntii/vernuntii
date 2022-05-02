using System.Collections.Generic;
using Vernuntii.MessageConventions;
using Vernuntii.MessageConventions.MessageIndicators;
using Vernuntii.MessagesProviders;
using Vernuntii.MessageVersioning;
using Vernuntii.SemVer;
using Vernuntii.VersionTransformers;
using Xunit;

namespace Vernuntii.VersioningPresets
{
    public class RightShiftWhenZeroMajorTests
    {
        private static IEnumerable<object[]> BuildIncrementShouldTransformVersionsGenerator()
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
        public void BuildIncrementShouldTransformVersions(ISemanticVersion startVersion, bool expectedRightShift, ISemanticVersion expectedVersion)
        {
            var preset = new VersioningPreset() {
                IncrementMode = VersionIncrementMode.Successive,
                MessageConvention = new MessageConvention() {
                    MajorIndicators = new[] { TruthyMessageIndicator.Default }
                },
                RightShiftWhenZeroMajor = true,
            };

            var builder = new VersionIncrementBuilder();

            var context = new MessageVersioningContext(new SemanticVersionCalculationOptions() {
                StartVersion = startVersion,
                VersioningPreset = preset
            });

            var nextVersion = builder.BuildIncrement(new Message(), context).TransformVersion(startVersion);
            Assert.Equal(expectedRightShift, context.IsVersionIndicationRightShifted);
            Assert.Equal(expectedVersion, nextVersion);
        }
    }
}
