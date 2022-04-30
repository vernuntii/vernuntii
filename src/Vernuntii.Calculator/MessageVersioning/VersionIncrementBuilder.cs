using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions.MessageIndicators;
using Vernuntii.MessagesProviders;
using Vernuntii.VersioningPresets;
using Vernuntii.VersionTransformers;

namespace Vernuntii.MessageVersioning
{
    internal class VersionIncrementBuilder
    {
        private static bool IsMessageIndicating(IEnumerable<IMessageIndicator>? messageIndicators, string? message, VersionPart partToIndicate)
        {
            var enumerator = messageIndicators?.GetEnumerator();

            if (enumerator is not null && enumerator.MoveNext()) {
                do {
                    if (enumerator.Current.IsMessageIndicating(message, partToIndicate)) {
                        return true;
                    }
                } while (enumerator.MoveNext());
            }

            return false;
        }

        private readonly IVersioningPreset _options;

        public VersionIncrementBuilder(IVersioningPreset options) =>
            _options = options ?? throw new ArgumentNullException(nameof(options));

        private bool IsMessageIndicatingMajor(string? message) =>
            IsMessageIndicating(_options.MessageConvention?.MajorIndicators, message, VersionPart.Major);

        private bool IsMessageIndicatingMinor(string? message) =>
            IsMessageIndicating(_options.MessageConvention?.MinorIndicators, message, VersionPart.Minor);

        private bool IsMessageIndicatingPatch(string? message) =>
            IsMessageIndicating(_options.MessageConvention?.PatchIndicators, message, VersionPart.Patch);

        public IEnumerable<ISemanticVersionTransformer> BuildIncrement(IMessage message, MessageVersioningContext context)
        {
            var messageContent = message.Content;

            bool isMessageIncrementingMajor = IsMessageIndicatingMajor(messageContent);

            bool isMessageIncrementingMinor = isMessageIncrementingMajor
                ? false
                : IsMessageIndicatingMinor(messageContent);

            bool isMessageIncrementingPatch = isMessageIncrementingMajor || isMessageIncrementingMinor
                ? false
                : IsMessageIndicatingPatch(messageContent);

            var isPostVersionPreRelease = context.VersionCalculationOptions.IsPostVersionPreRelease;
            var startVersionCoreAlreadyReleased = context.VersionCalculationOptions.StartVersionCoreAlreadyReleased;
            bool isHeightConventionApplicable = _options.HeightConvention is not null && _options.HeightConvention.Position != HeightIdentifierPosition.None;
            bool allowUnlimitedIncrements = _options.IncrementMode == VersionIncrementMode.Successive;

            if (_options.IncrementMode != VersionIncrementMode.None) {
                var allowIncrementBecauseReleaseOrDisabledHeight = !isPostVersionPreRelease || !isHeightConventionApplicable;

                if (isMessageIncrementingMajor
                    // and if version core is reserved we want to increment at least once
                    && ((startVersionCoreAlreadyReleased && !context.DoesCurrentVersionContainsMajorIncrement)
                        // or 
                        || (allowIncrementBecauseReleaseOrDisabledHeight
                            && (allowUnlimitedIncrements || !context.DoesCurrentVersionContainsMajorIncrement)))) {
                    yield return NextMajorVersionTransformer.Default;
                } else if (isMessageIncrementingMinor
                    && ((startVersionCoreAlreadyReleased && !context.DoesCurrentVersionContainsMinorIncrement)
                        || (allowIncrementBecauseReleaseOrDisabledHeight
                            && (allowUnlimitedIncrements || !context.DoesCurrentVersionContainsMinorIncrement)))) {
                    yield return NextMinorVersionTransformer.Default;
                } else if (isMessageIncrementingPatch
                    && ((startVersionCoreAlreadyReleased && !context.DoesCurrentVersionContainsPatchIncrement)
                        || (allowIncrementBecauseReleaseOrDisabledHeight
                            && (allowUnlimitedIncrements || !context.DoesCurrentVersionContainsPatchIncrement)))) {
                    yield return NextPatchVersionTransformer.Default;
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
