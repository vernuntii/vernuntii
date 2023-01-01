using FluentAssertions;
using Vernuntii.SemVer;
using Vernuntii.VersionPersistence.Presentation.Serializers;

namespace Vernuntii.VersionPersistence.Presentation
{
    public class VersionCacheStringBuilderTests
    {
        [Theory]
        [InlineData(new object[] { VersionPresentationKind.Value, VersionPresentationView.Text })]
        [InlineData(new object[] { VersionPresentationKind.Value, VersionPresentationView.Json })]
        [InlineData(new object[] { VersionPresentationKind.Value, VersionPresentationView.Yaml })]
        [InlineData(new object[] { VersionPresentationKind.Complex, VersionPresentationView.Text })]
        [InlineData(new object[] { VersionPresentationKind.Complex, VersionPresentationView.Yaml })]
        public void ToString_should_not_end_with_newline(
            VersionPresentationKind presentationKind,
            VersionPresentationView presentationView)
        {
            var versionCache = new VersionCache(SemanticVersion.Zero);

            var formattedVersion = new VersionCacheStringBuilder(versionCache)
                .UsePresentationKind(presentationKind)
                .UsePresentationParts(VersionCacheStringBuilder.DefaultPresentationPart)
                .UsePresentationView(presentationView)
                .ToString() ?? throw new InvalidOperationException();

            formattedVersion.Should().NotEndWith("\r");
            formattedVersion.Should().NotEndWith("\n");
        }
    }
}
