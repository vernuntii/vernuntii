using Vernuntii.VersionFoundation;
using Vernuntii.VersionPresentation.Serializers;

namespace Vernuntii.VersionPresentation
{
    /// <summary>
    /// String builder for <see cref="IVersionPresentation"/>.
    /// </summary>
    public class VersionPresentationStringBuilder
    {
        /// <summary>
        /// Default presentation kind: <see cref="VersionPresentationKind.Value"/>
        /// </summary>
        public const VersionPresentationKind DefaultPresentationKind = VersionPresentationKind.Value;

        /// <summary>
        /// Default presentation part: <see cref="VersionPresentationPart.SemanticVersion"/>
        /// </summary>
        public const VersionPresentationPart DefaultPresentationPart = VersionPresentationPart.SemanticVersion;

        /// <summary>
        /// Default presentation serializer: <see cref="VersionPresentationView.Text"/>
        /// </summary>
        public const VersionPresentationView DefaultPresentationSerializer = VersionPresentationView.Text;

        private readonly IVersionFoundation _presentationFoundation;
        private VersionPresentationKind _presentationKind = DefaultPresentationKind;
        private VersionPresentationPart _presentationParts = DefaultPresentationPart;
        private VersionPresentationView _presentationView = DefaultPresentationSerializer;

        /// <summary>
        /// Creates an instance of <see cref="VersionPresentationStringBuilder"/> with following defaults:
        /// <code>
        /// <see cref="VersionPresentationKind.Value"/>
        /// <br/><see cref="VersionPresentationPart.SemanticVersion"/>
        /// <br/><see cref="VersionPresentationView.Text"/>
        /// </code>
        /// </summary>
        /// <param name="presentationFoundation"></param>
        public VersionPresentationStringBuilder(IVersionFoundation presentationFoundation) =>
            _presentationFoundation = presentationFoundation ?? throw new ArgumentNullException(nameof(presentationFoundation));

        /// <summary>
        /// Uses presentation kind.
        /// </summary>
        /// <param name="presentationKind"></param>
        public VersionPresentationStringBuilder UsePresentationKind(VersionPresentationKind presentationKind)
        {
            _presentationKind = presentationKind;
            return this;
        }

        /// <summary>
        /// Uses presentation part.
        /// </summary>
        /// <param name="presentationParts"></param>
        /// <returns></returns>
        public VersionPresentationStringBuilder UsePresentationPart(VersionPresentationPart presentationParts)
        {
            _presentationParts = presentationParts;
            return this;
        }

        /// <summary>
        /// Uses presentation serializer.
        /// </summary>
        /// <param name="presentationView"></param>
        /// <returns></returns>
        public VersionPresentationStringBuilder UsePresentationView(VersionPresentationView presentationView)
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
            var complexVersionPresentation = VersionPresentation.Create(_presentationFoundation, _presentationParts);
            object? versionPresentation;

            if (_presentationKind == VersionPresentationKind.Value) {
                versionPresentation = complexVersionPresentation.GetValue(_presentationParts);
            } else if (_presentationKind == VersionPresentationKind.Complex) {
                versionPresentation = complexVersionPresentation;
            } else {
                throw new NotSupportedException($"Presentation kind \"{_presentationKind}\" not supported");
            }

            if (versionPresentation is null) {
                return null;
            }

            return VersionPresentationSerializerFactory
                .CreateSerializer(_presentationView)
                .SerializeSemanticVersion(versionPresentation, _presentationKind, _presentationParts);
        }
    }
}
