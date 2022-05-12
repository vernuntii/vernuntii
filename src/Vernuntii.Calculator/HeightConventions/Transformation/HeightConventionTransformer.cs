using Vernuntii.SemVer;

namespace Vernuntii.HeightConventions.Transformation
{
    /// <summary>
    /// Transforms dotted identifier by convention to make height available and accessible through index.
    /// </summary>
    public class HeightConventionTransformer
    {
        private static string HeightRuleHintSentence(int dots) =>
            $"Either add a height rule for {dots} dots or change height position to none.";

        /// <summary>
        /// The convention to use in transformation.
        /// </summary>
        public IHeightConvention Convention { get; }
        /// <summary>
        /// The height rules used in transformation.
        /// </summary>
        public IHeightRuleDictionary Rules { get; }
        /// <summary>
        /// The height placeholder parser used in transformation.
        /// </summary>
        public IHeightPlaceholderParser PlaceholderParser { get; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="heightConvention"></param>
        /// <param name="heightPlaceholderParser"></param>
        public HeightConventionTransformer(IHeightConvention heightConvention, IHeightPlaceholderParser? heightPlaceholderParser = null)
        {
            Convention = heightConvention;
            Rules = Convention.Rules ?? HeightRuleDictionary.Empty;
            PlaceholderParser = heightPlaceholderParser ?? HeightPlaceholderParser.Default;
        }

        /// <summary>
        /// Gets the identifier with containing height.
        /// </summary>
        /// <param name="dottedIdentifier"></param>
        /// <param name="dotSplittedIdentifiers"></param>
        /// <returns>
        /// Dot-splitted identifiers representing the
        /// template containing one empty identifier.
        /// </returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        protected virtual HeightConventionTransformResult TransformIdentifier(
            string dottedIdentifier,
            IReadOnlyList<string> dotSplittedIdentifiers)
        {
            HashSet<int> usedDots = new HashSet<int>();

            applyNextRule:
            var dots = Math.Max(0, dotSplittedIdentifiers.Count - 1);

            if (!Rules.TryGetValue(dots, out var heightRule)) {
                throw new InvalidOperationException($"A height rule for {dots} dots does not exist. {HeightRuleHintSentence(dots)}");
            }

            var heightTemplateIdentifiers = heightRule.Template.Split('.');

            var placeholderParseResults = heightTemplateIdentifiers.Select(identifier => {
                var placeholderType = PlaceholderParser.Parse(identifier, out var content);
                return new { Content = content, PlaceholderType = placeholderType };
            }).ToArray();

            var assumedIdentifiers = new string[heightTemplateIdentifiers.Length];
            var emptyPlaceholderCounter = 0;
            var assumedHeightIndex = -1;
            var assumedDots = placeholderParseResults.Length - 1;

            for (var i = 0; i < assumedIdentifiers.Length; i++) {
                var placeholderParseResult = placeholderParseResults[i];
                string? result = null;

                if (placeholderParseResult.PlaceholderType == HeightPlaceholderType.Empty) {
                    if (HasEmptyPlaceholder()) {
                        throw new NotSupportedException("Only one empty identifier (e.g. \"{y}.\") is allowed in expansion");
                    }

                    emptyPlaceholderCounter++;
                    EnsureEitherExpandingOrTemplating();
                } else if (placeholderParseResult.PlaceholderType == HeightPlaceholderType.Identifiers) {
                    IncreaseAssumedDots(dots);
                    result = dottedIdentifier;
                } else if (placeholderParseResult.PlaceholderType == HeightPlaceholderType.IdentifierIndex) {
                    result = dotSplittedIdentifiers[(int)placeholderParseResult.Content!];
                } else if (placeholderParseResult.PlaceholderType == HeightPlaceholderType.Height) {
                    if (IsHeightIndicated()) {
                        throw new NotSupportedException("Only one height indicator is allowed when templating");
                    }

                    assumedHeightIndex = i;
                    EnsureEitherExpandingOrTemplating();
                    assumedIdentifiers[i] = dotSplittedIdentifiers[i];
                    // We know that height can be empty or even malformed.
                    continue;
                } else {
                    throw new InvalidOperationException("Bad placeholder type");
                }

                if (string.IsNullOrEmpty(result)) {
                    result = "-";
                }

                assumedIdentifiers[i] = result;
            }

            if (!IsHeightIndicated()) {
                if (!usedDots.Add(dots)) {
                    throw new InvalidOperationException($"Cyclic dependency detected (Dots = {dots})");
                }

                dotSplittedIdentifiers = assumedIdentifiers;
                goto applyNextRule;
            }

            return new HeightConventionTransformResult(Convention, assumedIdentifiers, assumedHeightIndex);

            bool HasEmptyPlaceholder() => emptyPlaceholderCounter > 0;
            bool IsHeightIndicated() => assumedHeightIndex >= 0;

            void EnsureEitherExpandingOrTemplating()
            {
                if (HasEmptyPlaceholder() && IsHeightIndicated()) {
                    throw new InvalidOperationException("The height indicator cannot be used when expanding");
                }
            }

            void IncreaseAssumedDots(int dots)
            {
                assumedDots += dots;
                var tooManyDots = assumedDots - 1 - dots;

                if (tooManyDots > 0) {
                    throw new InvalidOperationException($"You can only expand by a total of one identifier");
                }
            }
        }

        /// <summary>
        /// Gets the identifier with containing height.
        /// </summary>
        /// <param name="version"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public HeightConventionTransformResult Transform(ISemanticVersion version)
        {
            string dottedIdentifier;
            IReadOnlyList<string> dotSplittedIdentifiers;

            if (Convention.Position == HeightIdentifierPosition.PreRelease) {
                dottedIdentifier = version.PreRelease;
                dotSplittedIdentifiers = version.PreReleaseIdentifiers;
            } else if (Convention.Position == HeightIdentifierPosition.Build) {
                dottedIdentifier = version.Build;
                dotSplittedIdentifiers = version.BuildIdentifiers;
            } else {
                throw new InvalidOperationException($"The height position \"{Convention.Position}\" does not exist");
            }

            return TransformIdentifier(dottedIdentifier, dotSplittedIdentifiers);
        }

        /// <summary>
        /// Gets the identifier with containing height.
        /// </summary>
        /// <param name="dottedIdentifier"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public HeightConventionTransformResult Transform(string dottedIdentifier) =>
            TransformIdentifier(dottedIdentifier, dottedIdentifier.Split('.'));

        /// <summary>
        /// Gets the identifier with containing height.
        /// </summary>
        /// <param name="dotSplittedIdentifiers"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public HeightConventionTransformResult Transform(IReadOnlyList<string> dotSplittedIdentifiers) =>
            TransformIdentifier(string.Join('.', dotSplittedIdentifiers), dotSplittedIdentifiers);
    }
}
