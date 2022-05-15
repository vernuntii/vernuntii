using Moq;
using Vernuntii.SemVer;
using Vernuntii.VersionFoundation;
using Vernuntii.VersionPresentation.Serializers;
using Xunit;

namespace Vernuntii.VersionPresentation
{
    public class SemanticVersionPresentationStringBuilderTests
    {
        [Theory]
        [InlineData(new object[] { SemanticVersionPresentationKind.Value, SemanticVersionPresentationView.Text })]
        [InlineData(new object[] { SemanticVersionPresentationKind.Value, SemanticVersionPresentationView.Json })]
        [InlineData(new object[] { SemanticVersionPresentationKind.Value, SemanticVersionPresentationView.Yaml })]
        [InlineData(new object[] { SemanticVersionPresentationKind.Complex, SemanticVersionPresentationView.Text })]
        [InlineData(new object[] { SemanticVersionPresentationKind.Complex, SemanticVersionPresentationView.Yaml })]
        public void BuildStringShouldNotContainNewline(
            SemanticVersionPresentationKind presentationKind,
            SemanticVersionPresentationView presentationView)
        {
            var foundation = Mock.Of<IVersionFoundation>(x =>
                x.Version.Equals(SemanticVersion.Zero));

            var formattedVersion = new SemanticVersionPresentationStringBuilder(foundation)
                .UsePresentationKind(presentationKind)
                .UsePresentationPart(SemanticVersionPresentationPart.SemanticVersion)
                .UsePresentationView(presentationView)
                .BuildString() ?? throw new InvalidOperationException();

            Assert.DoesNotContain("\r", formattedVersion, StringComparison.InvariantCulture);
            Assert.DoesNotContain("\n", formattedVersion, StringComparison.InvariantCulture);
        }
    }
}
