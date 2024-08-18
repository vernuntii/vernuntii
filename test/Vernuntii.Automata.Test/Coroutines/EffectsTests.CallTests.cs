namespace Vernuntii.Coroutines;

partial class EffectsTests
{
    public class CallTests
    {
        [Fact]
        public async Task AsyncCallReturningConstant_Suceeds()
        {
            const int expectedResult = 1;
            var result = await Coroutine.Start(() => Call(async () => expectedResult)).ConfigureAwait(false);
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task AwaitingAsyncCallReturningConstant_Suceeds()
        {
            const int expectedResult = 1;
            var result = await Coroutine.Start(async () => await Call(async () => expectedResult)).ConfigureAwait(false);
            Assert.Equal(expectedResult, result);
        }
    }
}
