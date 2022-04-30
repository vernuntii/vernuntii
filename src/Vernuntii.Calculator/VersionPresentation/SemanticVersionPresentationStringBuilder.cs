using Vernuntii.VersionFoundation;
using Vernuntii.VersionPresentation.Serializers;

namespace Vernuntii.VersionPresentation
{
    /// <summary>
    /// String builder for <see cref="ISemanticVersionPresentation"/>.
    /// </summary>
    public class SemanticVersionPresentationStringBuilder
    {
        /// <summary>
        /// Default presentation kind: <see cref="SemanticVersionPresentationKind.Value"/>
        /// </summary>
        public const SemanticVersionPresentationKind DefaultPresentationKind = SemanticVersionPresentationKind.Value;

        /// <summary>
        /// Default presentation part: <see cref="SemanticVersionPresentationPart.SemanticVersion"/>
        /// </summary>
        public const SemanticVersionPresentationPart DefaultPresentationPart = SemanticVersionPresentationPart.SemanticVersion;

        /// <summary>
        /// Default presentation serializer: <see cref="SemanticVersionPresentationView.Text"/>
        /// </summary>
        public const SemanticVersionPresentationView DefaultPresentationSerializer = SemanticVersionPresentationView.Text;

        private readonly ISemanticVersionFoundation _presentationFoundation;
        private SemanticVersionPresentationKind _presentationKind = DefaultPresentationKind;
        private SemanticVersionPresentationPart _presentationParts = DefaultPresentationPart;
        private SemanticVersionPresentationView _presentationView = DefaultPresentationSerializer;

        /// <summary>
        /// Creates an instance of <see cref="SemanticVersionPresentationStringBuilder"/> with following defaults:
        /// <code>
        /// <see cref="SemanticVersionPresentationKind.Value"/>
        /// <br/><see cref="SemanticVersionPresentationPart.SemanticVersion"/>
        /// <br/><see cref="SemanticVersionPresentationView.Text"/>
        /// </code>
        /// </summary>
        /// <param name="presentationFoundation"></param>
        public SemanticVersionPresentationStringBuilder(ISemanticVersionFoundation presentationFoundation) =>
            _presentationFoundation = presentationFoundation ?? throw new ArgumentNullException(nameof(presentationFoundation));

        /// <summary>
        /// Uses presentation kind.
        /// </summary>
        /// <param name="presentationKind"></param>
        public SemanticVersionPresentationStringBuilder UsePresentationKind(SemanticVersionPresentationKind presentationKind)
        {
            _presentationKind = presentationKind;
            return this;
        }

        /// <summary>
        /// Uses presentation part.
        /// </summary>
        /// <param name="presentationParts"></param>
        /// <returns></returns>
        public SemanticVersionPresentationStringBuilder UsePresentationPart(SemanticVersionPresentationPart presentationParts)
        {
            _presentationParts = presentationParts;
            return this;
        }

        /// <summary>
        /// Uses presentation serializer.
        /// </summary>
        /// <param name="presentationView"></param>
        /// <returns></returns>
        public SemanticVersionPresentationStringBuilder UsePresentationView(SemanticVersionPresentationView presentationView)
        {
            _presentationView = presentationView;
            return this;
        }

        /// <summary>
        /// Builds string.
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public string? BuildString()
        {
            var complexVersionPresentation = SemanticVersionPresentation.Create(_presentationFoundation, _presentationParts);
            object? versionPresentation;

            if (_presentationKind == SemanticVersionPresentationKind.Value) {
                versionPresentation = complexVersionPresentation.GetValue(_presentationParts);
            } else if (_presentationKind == SemanticVersionPresentationKind.Complex) {
                versionPresentation = complexVersionPresentation;
            } else {
                throw new NotSupportedException($"Presentation kind \"{_presentationKind}\" not supported");
            }

            if (versionPresentation is null) {
                return null;
            }

            return SemanticVersionPresentationSerializerFactory
                .CreateSerializer(_presentationView)
                .SerializeSemanticVersion(versionPresentation, _presentationKind, _presentationParts);
        }
    }
}
