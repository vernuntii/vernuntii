using Moq;
using Vernuntii.SemVer;
using Xunit;

namespace Vernuntii.HeightConventions.Transformation
{
    public class HeightConventionTransformerTests
    {
        [Fact]
        public void Transform_throws_because_HeightIdentifierPosition_None_does_not_exist()
        {
            Mock<IHeightPlaceholderParser> placeholderParserMock = new();
            var heightConvention = Mock.Of<IHeightConvention>(x => x.Position == HeightIdentifierPosition.None);
            HeightConventionTransformer transformer = new(heightConvention, Mock.Of<IHeightPlaceholderParser>());

            var invalidOperationError = Assert.IsType<InvalidOperationException>(Record.Exception(() => transformer.Transform(Mock.Of<ISemanticVersion>())));
            Assert.Equal("The height position \"None\" does not exist", invalidOperationError.Message, StringComparer.InvariantCulture);
        }

        [Fact]
        public void Transform_throws_due_to_zero_dot_rules()
        {
            Mock<IHeightPlaceholderParser> placeholderParserMock = new();

            var heightConvention = Mock.Of<IHeightConvention>(x =>
                x.Position == HeightIdentifierPosition.PreRelease
                && x.Rules == HeightRuleDictionary.Empty);

            HeightConventionTransformer transformer = new(heightConvention, Mock.Of<IHeightPlaceholderParser>());

            var version = Mock.Of<ISemanticVersion>(x =>
                x.PreRelease == ""
                && x.PreReleaseIdentifiers == new string[] { "" });

            var invalidOperationError = Assert.IsType<InvalidOperationException>(Record.Exception(() => transformer.Transform(version)));
            Assert.Contains("A height rule for 0 dots does not exist", invalidOperationError.Message, StringComparison.InvariantCulture);
        }

        [Theory]
        [InlineData(new object[] { "{y}." })]
        [InlineData(new object[] { ".{y}" })]
        public void Transform_throws_because_dot_position_before_template_is_empty(string template)
        {
            Mock<IHeightPlaceholderParser> placeholderParserMock = new();

            var heightConvention = Mock.Of<IHeightConvention>(x =>
                x.Position == HeightIdentifierPosition.PreRelease
                && x.Rules == new HeightRuleDictionary(new[]{
                    new HeightRule(1, template)
                }));

            HeightConventionTransformer transformer = new(heightConvention, DefaultHeightPlaceholderParser);

            var version = Mock.Of<ISemanticVersion>(x =>
                x.PreRelease == "."
                && x.PreReleaseIdentifiers == new string[] { "", "" });

            var invalidOperationError = Assert.IsType<InvalidOperationException>(Record.Exception(() => transformer.Transform(version)));
            Assert.Contains("The height indicator cannot be used when expanding", invalidOperationError.Message, StringComparison.InvariantCulture);
        }

        [Fact]
        public void Transform_does_only_transform_first_position_of_pre_release()
        {
            Mock<IHeightPlaceholderParser> placeholderParserMock = new();

            HeightConvention heightConvention = new(HeightIdentifierPosition.PreRelease) {
                Rules = new HeightRuleDictionary(new[] {
                    new HeightRule(0, "{y}")
                })
            };

            HeightConventionTransformer transformer = new(heightConvention, DefaultHeightPlaceholderParser);

            var version = Mock.Of<ISemanticVersion>(x =>
                x.PreRelease == "2"
                && x.PreReleaseIdentifiers == new string[] { "2" });

            var transformResult = transformer.Transform(version);
            Assert.Equal(heightConvention, transformResult.Convention);
            Assert.Equal(new string[] { "2" }, transformResult.Identifiers);
            Assert.Equal(0, transformResult.HeightIndex);
        }
    }
}
