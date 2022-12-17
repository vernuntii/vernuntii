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
            var versioningPreset = context.VersioningPreset;
            var messageConvention = versioningPreset.MessageConvention;

            var isMessageIncrementingMajor = messageConvention.IsMessageIndicatingMajor(messageContent);
            var isMessageIncrementingMinor = false;
            var isMessageIncrementingPatch = false;

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
            var isPostVersionPreRelease = context.IsPostVersionPreRelease;
            var startVersionCoreAlreadyReleased = context.IsStartVersionCoreAlreadyReleased;
            var isHeightConventionApplicable = heightConvention is not null && heightConvention.Position != HeightIdentifierPosition.None;
            var allowUnlimitedIncrements = incrementMode == VersionIncrementMode.Successive;
            var skipVersionCoreIncrementationDueToPreReleaseAdaption = false;

            if (context.IsPreReleaseAdaptionOfStartVersionCoreEquivalentCurrentVersionRequired && !startVersionCoreAlreadyReleased && context.StartVersion.IsPreRelease) {
                if (isMessageIncrementingMajor && context.IsRightSideOfMajorOfCurrentVersionCoreZeroed) {
                    skipVersionCoreIncrementationDueToPreReleaseAdaption = true;
                }

                if (isMessageIncrementingMinor && context.IsRightSideOfMinorOfCurrentVersionCoreZeroed) {
                    skipVersionCoreIncrementationDueToPreReleaseAdaption = true;
                }

                if (isMessageIncrementingPatch) {
                    skipVersionCoreIncrementationDueToPreReleaseAdaption = true;
                }

                if (skipVersionCoreIncrementationDueToPreReleaseAdaption) {
                    yield return context.PostVersionPreReleaseTransformer;
                }
            }

            if (incrementMode != VersionIncrementMode.None && !skipVersionCoreIncrementationDueToPreReleaseAdaption) {
                var allowVersionCoreIncrementationBecauseReleaseOrDisabledHeight = !isPostVersionPreRelease || !isHeightConventionApplicable;

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
                    // Only if the message indicates an incrementation and ..
                    return isMessageIncrementingVersion
                        // .. if version core is reserved we want to increment at least once, ..
                        && ((startVersionCoreAlreadyReleased && !doesCurrentVersionContainsIncrement)
                            // .. or if incrementing the version core is allowed by convention
                            || (allowVersionCoreIncrementationBecauseReleaseOrDisabledHeight
                                && (allowUnlimitedIncrements || !doesCurrentVersionContainsIncrement)));
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
