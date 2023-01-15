using Vernuntii.VersionPersistence.Presentation.Serializers;

namespace Vernuntii.VersionPersistence.Presentation
{
    /// <summary>
    /// String builder for <see cref="IVersionCache"/>.
    /// </summary>
    public sealed class VersionCacheStringBuilder
    {
        /// <summary>
        /// The presentation kind. (default <see cref="VersionPresentationKind.Value"/>)
        /// </summary>
        public const VersionPresentationKind DefaultPresentationKind = VersionPresentationKind.Value;

        /// <summary>
        /// Consits of <see cref="VersionCachePart.SemanticVersion"/>.
        /// </summary>
        public static readonly VersionPresentationParts DefaultPresentationPart = VersionPresentationParts.Of(VersionCacheParts.SemanticVersion);

        /// <summary>
        /// The presentation view. (default <see cref="VersionPresentationView.Text"/>)
        /// </summary>
        public const VersionPresentationView DefaultPresentationSerializer = VersionPresentationView.Text;

        private readonly IVersionCache _presentationCache;
        private VersionPresentationKind _presentationKind = DefaultPresentationKind;
        private VersionPresentationParts _presentationParts = DefaultPresentationPart;
        private VersionPresentationView _presentationView = DefaultPresentationSerializer;

        /// <summary>
        /// Creates an instance of <see cref="VersionCacheStringBuilder"/> with following defaults:
        /// <code>
        /// <see cref="VersionPresentationKind.Value"/>
        /// <br/><see cref="VersionCachePart.SemanticVersion"/>
        /// <br/><see cref="VersionPresentationView.Text"/>
        /// </code>
        /// </summary>
        /// <param name="presentationCache"></param>
        public VersionCacheStringBuilder(IVersionCache presentationCache) =>
            _presentationCache = presentationCache ?? throw new ArgumentNullException(nameof(presentationCache));

        /// <summary>
        /// Uses presentation kind.
        /// </summary>
        /// <param name="presentationKind"></param>
        public VersionCacheStringBuilder UsePresentationKind(VersionPresentationKind presentationKind)
        {
            _presentationKind = presentationKind;
            return this;
        }

        /// <summary>
        /// Uses presentation part.
        /// </summary>
        /// <param name="presentationParts"></param>
        /// <returns></returns>
        public VersionCacheStringBuilder UsePresentationParts(VersionPresentationParts presentationParts)
        {
            _presentationParts = presentationParts;
            return this;
        }

        /// <summary>
        /// Uses presentation serializer.
        /// </summary>
        /// <param name="presentationView"></param>
        /// <returns></returns>
        public VersionCacheStringBuilder UsePresentationView(VersionPresentationView presentationView)
        {
            _presentationView = presentationView;
            return this;
        }

        /// <summary>
        /// Builds string.
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public override string ToString() =>
            VersionPresentationSerializerFactory
                .CreateSerializer(_presentationView)
                .SerializeSemanticVersion(_presentationCache, _presentationKind, _presentationParts);
    }
}
