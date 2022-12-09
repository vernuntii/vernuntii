using Vernuntii.SemVer.Parser;
using Vernuntii.SemVer.Parser.Parsers;

namespace Vernuntii.HeightConventions.Transformation
{
    /// <summary>
    /// The height identifier result consisting of 
    /// <see cref="Identifiers"/> and
    /// <see cref="HeightIndex"/>.
    /// </summary>
    public sealed class HeightConventionTransformResult
    {
        /// <summary>
        /// The convention that has been used in transformation.
        /// </summary>
        public IHeightConvention Convention { get; }
        /// <summary>
        /// The identifiers.
        /// </summary>
        public string[] Identifiers { get; }
        /// <summary>
        /// The height index.
        /// </summary>
        public int HeightIndex { get; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="convention"></param>
        /// <param name="identifiers"></param>
        /// <param name="heightIndex"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HeightConventionTransformResult(IHeightConvention convention, string[] identifiers, int heightIndex)
        {
            Convention = convention ?? throw new ArgumentNullException(nameof(convention));
            Identifiers = identifiers ?? throw new ArgumentNullException(nameof(identifiers));

            if (heightIndex < 0 || heightIndex >= identifiers.Length) {
                throw new ArgumentOutOfRangeException(nameof(heightIndex), $"Height index ({heightIndex}) is smaller than zero or greater than the number of identifiers");
            }

            HeightIndex = heightIndex;
        }

        /// <summary>
        /// Gehts height from <see cref="Identifiers"/>.
        /// </summary>
        public string GetHeight() =>
            Identifiers[HeightIndex];

        private SemanticVersionPart GetVersionPart() => Convention.Position switch {
            HeightIdentifierPosition.PreRelease => SemanticVersionPart.PreRelease,
            HeightIdentifierPosition.Build => SemanticVersionPart.Build,
            _ => SemanticVersionPart.None
        };

        /// <summary>
        /// Tries to parse height.
        /// </summary>
        /// <param name="versionNumberParser"></param>
        /// <param name="height"></param>
        /// <returns>True if height is number.</returns>
        public bool TryParseHeight(INumericIdentifierParser versionNumberParser, out uint? height) =>
            versionNumberParser.TryParseNumericIdentifier(GetVersionPart(), GetHeight()).DeconstructSuccess(out height);
    }
}
