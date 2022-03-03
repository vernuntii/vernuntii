using Vernuntii.MessagesProviders;
using Vernuntii.MessageVersioning.MessageIndicators;
using Vernuntii.VersionTransformers;

namespace Vernuntii.MessageVersioning
{
    internal class VersionTransformerBuilder
    {
        /// <summary>
        /// A boolean indicating whether the current version contains
        /// at least one major increment towards the initial version.
        /// </summary>
        private static bool DoesCurrentVersionContainsMajorIncrement(MessageVersioningContext context) =>
            context.CurrentVersion.Major > context.CalculationOptions.StartVersion.Major;

        /// <summary>
        /// A boolean indicating whether the current version contains
        /// at least one minor increment towards the initial version.
        /// </summary>
        private static bool DoesCurrentVersionContainsMinorIncrement(MessageVersioningContext context) =>
            DoesCurrentVersionContainsMajorIncrement(context)
            || (context.CurrentVersion.Major == context.CalculationOptions.StartVersion.Major
                && context.CurrentVersion.Minor > context.CalculationOptions.StartVersion.Minor);

        /// <summary>
        /// A boolean indicating whether the current version contains
        /// at least one patch increment towards the initial version.
        /// </summary>
        private static bool DoesCurrentVersionContainsPatchIncrement(MessageVersioningContext context) =>
            DoesCurrentVersionContainsMajorIncrement(context)
            || DoesCurrentVersionContainsMinorIncrement(context)
            || (context.CurrentVersion.Major == context.CalculationOptions.StartVersion.Major
                && context.CurrentVersion.Minor == context.CalculationOptions.StartVersion.Minor
                && context.CurrentVersion.Patch > context.CalculationOptions.StartVersion.Patch);

        private readonly VersionTransformerBuilderOptions _options;

        public VersionTransformerBuilder(VersionTransformerBuilderOptions options) =>
            _options = options ?? throw new ArgumentNullException(nameof(options));

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

        private bool IsMessageIndicatingMajor(string? message) =>
            IsMessageIndicating(_options.MajorIndicators, message, VersionPart.Major);

        private bool IsMessageIndicatingMinor(string? message) =>
            IsMessageIndicating(_options.MinorIndicators, message, VersionPart.Minor);

        private bool IsMessageIndicatingPatch(string? message) =>
            IsMessageIndicating(_options.PatchIndicators, message, VersionPart.Patch);

        public ISemanticVersionTransformer BuildTransformer(IMessage message, MessageVersioningContext context)
        {
            var messageContent = message.Content;

            bool isMessageIncrementingMajor = IsMessageIndicatingMajor(messageContent);

            bool isMessageIncrementingMinor = isMessageIncrementingMajor
                ? false
                : IsMessageIndicatingMinor(messageContent);

            bool isMessageIncrementingPatch = isMessageIncrementingMajor || isMessageIncrementingMinor
                ? false
                : IsMessageIndicatingPatch(messageContent);

            bool incrementConsecutive = _options.IncrementMode == VersionIncrementMode.Consecutive;

            if (isMessageIncrementingMajor && !(incrementConsecutive && DoesCurrentVersionContainsMajorIncrement(context))) {
                return NextMajorVersionTransformer.Default;
            }

            if (isMessageIncrementingMinor && !(incrementConsecutive && DoesCurrentVersionContainsMinorIncrement(context))) {
                return NextMinorVersionTransformer.Default;
            }

            if (isMessageIncrementingPatch && !(incrementConsecutive && DoesCurrentVersionContainsPatchIncrement(context))) {
                return NextPatchVersionTransformer.Default;
            }

            if (context.CalculationOptions.IsPostVersionPreRelease) {
                // Next build height when post version is pre-release.
                return NextBuildNumberTransformer.Default;
            } else {
                return NoneVersionTransformer.Default;
            }
        }
    }
}
