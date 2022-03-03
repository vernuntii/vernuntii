using Vernuntii.MessageVersioning.HeightConventions;
using Vernuntii.MessageVersioning.HeightConventions.Ruling;

namespace Vernuntii.VersionTransformers
{
    internal class NextHeightNumberTransformer : ISemanticVersionTransformer
    {
        private static string HeightRuleHintSentence(int dots) =>
            $"Either add a height rule for {dots} dots or change height position to none.";

        private readonly IHeightConvention _heightConvention;
        private readonly IHeightPlaceholderParser _heightPlaceholderParser = HeightPlaceholderParser.Default;

        public NextHeightNumberTransformer(IHeightConvention heightConvention) =>
            _heightConvention = heightConvention ?? throw new ArgumentNullException(nameof(heightConvention));

        private void test(IReadOnlyList<string> versionIdentifiers, int dots, HeightRuleDictionary heightRules)
        {
            if (!heightRules.TryGetValue(dots, out var heightRule)) {
                throw new InvalidOperationException($"A height rule for {dots} dots does not exist. {HeightRuleHintSentence(dots)}");
            }

            var heightTemplateIdentifiers = heightRule.Template.Split('.');

            var placeholderParseResults = heightTemplateIdentifiers.Select(identifier => {
                var content = _heightPlaceholderParser.ParsePlaceholder(identifier, out var placeholderType);
                return new { Content = content, PlaceHolderType = placeholderType };
            });
        }

        public SemanticVersion TransformVersion(SemanticVersion version)
        {
            if (_heightConvention.Position == HeightPosition.None) {
                goto exit;
            }

            var versionIdentifiers = _heightConvention.Position switch {
                HeightPosition.PreRelease => version.PreReleaseIdentifiers,
                HeightPosition.Build => version.BuildIdentifiers,
                _ => throw new InvalidOperationException($"The height position \"{_heightConvention.Position}\" does not exist")
            };

            var heightRules = _heightConvention.Rules ?? HeightRuleDictionary.Empty;
            var dots = versionIdentifiers.Count;

            if (heightRules.Count == 0) {
                throw new InvalidOperationException($"Height feature is enabled but no rules are defined. {HeightRuleHintSentence(dots)}");
            }

            test(versionIdentifiers, dots, heightRules);

            exit:
            return version;
        }
    }
}
