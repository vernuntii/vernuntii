using Xunit;

namespace Vernuntii.SemVer.Parser
{
    public class IdentifierParseResultTest
    {
        public static IEnumerable<object[]> ValidParseResultShouldSucceedGenerator()
        {
            yield return new object[] { IdentifierParseResult<object>.ValidNull };
            yield return new object[] { IdentifierParseResult.ValidParse(default(object)) };
        }

        [Theory]
        [MemberData(nameof(ValidParseResultShouldSucceedGenerator))]
        public void ValidParseResultShouldSucceed(IdentifierParseResult<object> parseResult)
        {
            Assert.True(parseResult.Suceeded);
            Assert.False(parseResult.Failed);
        }

        public static IEnumerable<object[]> InvalidParseResultShouldSucceedGenerator()
        {
            yield return new object[] { IdentifierParseResult<object>.InvalidNull };
            yield return new object[] { IdentifierParseResult<object>.InvalidEmpty };
            yield return new object[] { IdentifierParseResult<object>.InvalidWhiteSpace };
            yield return new object[] { IdentifierParseResult.InvalidParse(default(object)) };
        }

        [Theory]
        [MemberData(nameof(InvalidParseResultShouldSucceedGenerator))]
        public void InvalidParseResultShouldFailed(IdentifierParseResult<object> parseResult)
        {
            Assert.True(parseResult.Failed);
            Assert.False(parseResult.Suceeded);
        }
    }
}
