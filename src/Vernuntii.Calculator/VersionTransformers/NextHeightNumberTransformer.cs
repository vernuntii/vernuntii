using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Vernuntii.HeightConventions;
using Vernuntii.HeightConventions.Transformation;
using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    internal class NextHeightNumberTransformer : IVersionTransformer
    {
        private readonly HeightConventionTransformer? _transformer;
        //private readonly IVersionExistenceChecker _versionExistenceChecker;
        private HeightConventionTransformResult? _transformResult;
        private uint? _parsedHeightNumber;

        bool IVersionTransformer.DoesNotTransform => false;

        public NextHeightNumberTransformer(
            HeightConventionTransformer transformer
            /*IVersionExistenceChecker versionExistenceChecker*/)
        {
            _transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
            //_versionExistenceChecker = versionExistenceChecker ?? throw new ArgumentNullException(nameof(versionExistenceChecker));
        }

        internal NextHeightNumberTransformer(
            HeightConventionTransformResult transformResult,
            uint? parsedHeightNumber
            /*IVersionExistenceChecker versionExistenceChecker*/)
        {
            _transformResult = transformResult ?? throw new ArgumentNullException(nameof(transformResult));
            //_versionExistenceChecker = versionExistenceChecker ?? throw new ArgumentNullException(nameof(versionExistenceChecker));
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
            var conventionInitialHeight = _transformResult.Convention.InitialHeight;
            var initialOrNextHeight = GetInitialOrNextHeight(_parsedHeightNumber, conventionInitialHeight);

            //if (_transformResult.Convention.HideInitialHeight
            //    && conventionInitialHeight == initialOrNextHeight) {
            //    // Replace identifiers with identifiers that does not contain the height.
            //    identifiers = GetIdentifiersWithoutHeight(_transformResult);
            //} else {
            identifiers[_transformResult.HeightIndex] = StringifyVersionNumber(initialOrNextHeight);
            //}

            if (_transformResult.Convention.Position == HeightIdentifierPosition.PreRelease) {
                return version.With().PreRelease(identifiers).ToVersion();
            } else {
                return version.With().Build(identifiers).ToVersion();
            }

            static uint GetInitialOrNextHeight(uint? height, uint initialHeight)
            {
                if (!height.HasValue) {
                    return initialHeight;
                }

                return height.Value + 1;
            }

            static string StringifyVersionNumber(uint versionNumber) =>
                versionNumber.ToString(CultureInfo.InvariantCulture);

            //static string[] GetIdentifiersWithoutHeight(HeightConventionTransformResult _transformResult)
            //{
            //    var identifiers = _transformResult.Identifiers;
            //    var identifiersLength = identifiers.Length;
            //    var identifiersWithoutHeightLength = identifiersLength - 1;
            //    var identifiersWithoutHeight = new string[identifiersWithoutHeightLength];
            //    var heightIndex = _transformResult.HeightIndex;

            //    if (heightIndex == 0) {
            //        Array.Copy(identifiers, 1, identifiersWithoutHeight, 0, identifiersWithoutHeightLength);
            //    } else if (heightIndex == identifiersWithoutHeightLength) {
            //        Array.Copy(identifiers, identifiersWithoutHeight, identifiersWithoutHeightLength);
            //    } else {
            //        Array.Copy(identifiers, 0, identifiersWithoutHeight, 0, heightIndex);
            //        Array.Copy(identifiers, heightIndex + 1, identifiersWithoutHeight, heightIndex, identifiersWithoutHeightLength - heightIndex);
            //    }

            //    // Replace identifiers with identifiers that does not contain the height anymore.
            //    return identifiersWithoutHeight;
            //}
        }
    }
}
