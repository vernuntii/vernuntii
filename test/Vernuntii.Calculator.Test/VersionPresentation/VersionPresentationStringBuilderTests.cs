using Moq;
using Vernuntii.SemVer;
using Vernuntii.VersionFoundation;
using Vernuntii.VersionPresentation.Serializers;
using Xunit;

namespace Vernuntii.VersionPresentation
{
    public class VersionPresentationStringBuilderTests
    {
        [Theory]
        [InlineData(new object[] { VersionPresentationKind.Value, VersionPresentationView.Text })]
        [InlineData(new object[] { VersionPresentationKind.Value, VersionPresentationView.Json })]
        [InlineData(new object[] { VersionPresentationKind.Value, VersionPresentationView.Yaml })]
        [InlineData(new object[] { VersionPresentationKind.Complex, VersionPresentationView.Text })]
        [InlineData(new object[] { VersionPresentationKind.Complex, VersionPresentationView.Yaml })]
        public void BuildStringShouldNotContainNewline(
            VersionPresentationKind presentationKind,
            VersionPresentationView presentationView)
        {
            var foundation = Mock.Of<IVersionFoundation>(x =>
                x.Version.Equals(SemanticVersion.Zero));

            var formattedVersion = new VersionPresentationStringBuilder(foundation)
                .UsePresentationKind(presentationKind)
                .UsePresentationPart(VersionPresentationPart.SemanticVersion)
                .UsePresentationView(presentationView)
                .BuildString() ?? throw new InvalidOperationException();

            Assert.DoesNotContain("\r", formattedVersion, StringComparison.InvariantCulture);
            Assert.DoesNotContain("\n", formattedVersion, StringComparison.InvariantCulture);
        }
    }
}
