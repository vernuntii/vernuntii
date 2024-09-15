namespace Vernuntii.Coroutines.Iterators;

partial class AsyncIteratorTests
{
    public class NonRelative
    {
        public class ReturnSynchronously : AbstractReturnSynchronously<int, int> {
            protected override Coroutine<int> Constant() => Coroutine.FromResult(ExpectedResult);
            protected override ValueTask<int> Unwrap(int resultWrapper) => new(resultWrapper);
            protected override ValueTask<Coroutine<int>> Unwrap(Coroutine<int> coroutine) => new(coroutine);
        }

        public class ReturnAfterDelay : AbstractReturnAfterDelay<int, int>
        {
            protected override async Coroutine<int> ConstantAfterDelay() {
                await Task.Delay(ContinueAfterTimeInMs).ConfigureAwait(false);
                return ExpectedResult;
            }

            protected override ValueTask<int> Unwrap(int resultWrapper) => new(resultWrapper);
            protected override ValueTask<Coroutine<int>> Unwrap(Coroutine<int> x) => new(x);

            [Fact]
            public override Task GetResult_Throws() => base.GetResult_Throws();
        }
    }
}
