using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Vernuntii.HeightConventions;
using Vernuntii.HeightConventions.Transformation;
using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    internal class NextHeightNumberTransformer : ISemanticVersionTransformer
    {
        private static uint GetStartOrNextHeight(uint? height, uint startHeight)
        {
            if (!height.HasValue) {
                return startHeight;
            }

            return height.Value + 1;
        }

        private static string StringifyVersionNumber(uint versionNumber) =>
            versionNumber.ToString(CultureInfo.InvariantCulture);

        private readonly HeightConventionTransformer? _transformer;
        private HeightConventionTransformResult? _transformResult;
        private uint? _parsedHeightNumber;

        bool ISemanticVersionTransformer.DoesNotTransform => false;

        public NextHeightNumberTransformer(HeightConventionTransformer transformer) =>
            _transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));

        internal NextHeightNumberTransformer(HeightConventionTransformResult transformResult, uint? parsedHeightNumber)
        {
            _transformResult = transformResult ?? throw new ArgumentNullException(nameof(transformResult));
            _parsedHeightNumber = parsedHeightNumber;
        }

        [MemberNotNull(nameof(_transformResult))]
        private void EnsureUsingTransformer(ISemanticVersion version)
        {
            if (_transformResult is not null) {
                return;
            }

            if (_transformer is null) {
                throw new InvalidOperationException("Transformer is not defined");
            }

            _transformResult = _transformer.Transform(version);
            _ = _transformResult.TryParseHeight(version.GetParserOrStrict().VersionParser, out _parsedHeightNumber);
        }

        public ISemanticVersion TransformVersion(ISemanticVersion version)
        {
            EnsureUsingTransformer(version);
            var identifiers = _transformResult.Identifiers;

            identifiers[_transformResult.HeightIndex] = StringifyVersionNumber(
                GetStartOrNextHeight(_parsedHeightNumber, _transformResult.Convention.StartHeight));

            if (_transformResult.Convention.Position == HeightIdentifierPosition.PreRelease) {
                return version.With().PreRelease(identifiers).ToVersion();
            } else {
                return version.With().Build(identifiers).ToVersion();
            }
        }
    }
}
