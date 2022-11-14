using Xunit;

namespace Vernuntii.HeightConventions.Transformation
{
    public class HeightPlaceholderParserTests
    {
        public static IEnumerable<object?[]> AllowedPlaceholderGenerator()
        {
            yield return new object?[] { HeightPlaceholderType.Empty, null, "" };
            yield return new object?[] { HeightPlaceholderType.Identifiers, null, "{}" };
            yield return new object?[] { HeightPlaceholderType.Height, null, "{y}" };
            yield return new object?[] { HeightPlaceholderType.IdentifierIndex, 0, "{0}" };
            yield return new object?[] { HeightPlaceholderType.IdentifierIndex, 1, "{1}" };
        }

        [Theory]
        [MemberData(nameof(AllowedPlaceholderGenerator))]
        public void ParseResultShouldMatch(HeightPlaceholderType expectedPlaceholderType, object? expectedPlaceholderContent, string placeholder)
        {
            HeightPlaceholderParser parser = new();
            HeightPlaceholderType actualPlaceholderType = parser.Parse(placeholder, out object? actualPlaceholderContent);

            Assert.Equal(expectedPlaceholderType, actualPlaceholderType);
            Assert.Equal(expectedPlaceholderContent, actualPlaceholderContent);
        }
    }
}
