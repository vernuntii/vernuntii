using System.Runtime.CompilerServices;
using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.MessagesProviders;
using Vernuntii.VersionTransformers;

namespace Vernuntii.VersionIncrementing
{
    internal class VersionIncrementBuilder : IVersionIncrementBuilder
    {
        public IEnumerable<IVersionTransformer> BuildIncrement(IMessage message, VersionIncrementContext context)
        {
            var messageContent = message.Content;
            var versioningPreset = context.VersionCalculationOptions.VersioningPreset;
            var messageConvention = versioningPreset.MessageConvention;

            bool isMessageIncrementingMajor = messageConvention.IsMessageIndicatingMajor(messageContent);
            bool isMessageIncrementingMinor = false;
            bool isMessageIncrementingPatch = false;

            if (isMessageIncrementingMajor) {
                if (context.CanFlowDownstreamMajor) {
                    isMessageIncrementingMajor = false;
                    isMessageIncrementingMinor = true;
                    context.IsVersionDownstreamFlowed = true;
                }
            } else {
                isMessageIncrementingMinor = messageConvention.IsMessageIndicatingMinor(messageContent);

                if (isMessageIncrementingMinor) {
                    if (context.CanFlowDownstreamMinor) {
                        isMessageIncrementingMinor = false;
                        isMessageIncrementingPatch = true;
                        context.IsVersionDownstreamFlowed = true;
                    }
                } else {
                    isMessageIncrementingPatch = messageConvention.IsMessageIndicatingPatch(messageContent);
                }
            }

            var heightConvention = versioningPreset.HeightConvention;
            var incrementMode = versioningPreset.IncrementMode;
            var isPostVersionPreRelease = context.VersionCalculationOptions.IsPostVersionPreRelease;
            var startVersionCoreAlreadyReleased = context.VersionCalculationOptions.StartVersionCoreAlreadyReleased;
            bool isHeightConventionApplicable = heightConvention is not null && heightConvention.Position != HeightIdentifierPosition.None;
            bool allowUnlimitedIncrements = incrementMode == VersionIncrementMode.Successive;

            if (incrementMode != VersionIncrementMode.None) {
                var allowIncrementBecauseReleaseOrDisabledHeight = !isPostVersionPreRelease || !isHeightConventionApplicable;

                if (CanIncrementVersion(
                    isMessageIncrementingVersion: isMessageIncrementingMajor,
                    doesCurrentVersionContainsIncrement: context.DoesCurrentVersionContainsMajorIncrement)) {
                    yield return NextMajorVersionTransformer.Default;
                } else if (CanIncrementVersion(
                    isMessageIncrementingVersion: isMessageIncrementingMinor,
                    doesCurrentVersionContainsIncrement: context.DoesCurrentVersionContainsMinorIncrement)) {
                    yield return NextMinorVersionTransformer.Default;
                } else if (CanIncrementVersion(
                    isMessageIncrementingVersion: isMessageIncrementingPatch,
                    doesCurrentVersionContainsIncrement: context.DoesCurrentVersionContainsPatchIncrement)) {
                    yield return NextPatchVersionTransformer.Default;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                bool CanIncrementVersion(
                    bool isMessageIncrementingVersion,
                    bool doesCurrentVersionContainsIncrement)
                {
                    return isMessageIncrementingVersion
                        // and if version core is reserved we want to increment at least once
                        && (startVersionCoreAlreadyReleased && !doesCurrentVersionContainsIncrement
                            // or 
                            || allowIncrementBecauseReleaseOrDisabledHeight
                                && (allowUnlimitedIncrements || !doesCurrentVersionContainsIncrement));
                }
            }

            if (isPostVersionPreRelease
                && isHeightConventionApplicable
                && (allowUnlimitedIncrements
                    || !context.DoesCurrentVersionContainsHeightIncrement)) {
                // Next height when post version is pre-release.
                yield return new NextHeightNumberTransformer(
                    context.DoesCurrentVersionContainsHeightIncrement.TransformResult,
                    context.DoesCurrentVersionContainsHeightIncrement.HeightNumber);
            }
        }
    }
}
