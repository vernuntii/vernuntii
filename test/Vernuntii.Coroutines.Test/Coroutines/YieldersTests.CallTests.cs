namespace Vernuntii.Coroutines;

partial class YieldersTests
{
    public class CallTests
    {
        const int ExpectedResult = 1;

        public static IEnumerable<object[]> AsyncCallReturningConstant_RunsThrough_Generator()
        {
            yield return [Coroutine.Start(() => Call(async () => {
                await Task.Delay(ContinueAfterTimeInMs).ConfigureAwait(false);
                return ExpectedResult;
            }))._coroutine];
            yield return [Call(async () => {
                await Task.Delay(ContinueAfterTimeInMs).ConfigureAwait(false);
                return ExpectedResult;
            })];
        }

        [Theory]
        [MemberData(nameof(AsyncCallReturningConstant_RunsThrough_Generator))]
        public async Task AsyncCallReturningConstant_RunsThrough(Coroutine<int> coroutine)
        {
            var result = await coroutine.ConfigureAwait(false);
            Assert.Equal(ExpectedResult, result);
        }

        [Fact]
        public async Task AwaitingAsyncCallReturningConstant_Suceeds()
        {
            var result = await Coroutine.Start(async () => await Call(async () => ExpectedResult)).ConfigureAwait(false);
            Assert.Equal(ExpectedResult, result);
        }

        [Fact]
        public async Task AwaitingAsyncCallWithClosure_Suceeds()
        {
            var result = await Coroutine.Start(async () => await Call(async (expectedResult) => expectedResult, ExpectedResult)).ConfigureAwait(false);
            Assert.Equal(ExpectedResult, result);
        }
    }
}
