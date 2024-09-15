namespace Vernuntii.Coroutines.Iterators;
partial class AsyncIteratorTests
{
    public class WithContext
    {
        static CoroutineContext _defaultContext = new();

        public class ReturnSynchronously : AbstractReturnSynchronously<int, int>
        {
            protected override Coroutine<int> Constant() => WithContext(_defaultContext, () => new Coroutine<int>(ExpectedResult));
            protected override ValueTask<int> Unwrap(int resultWrapper) => new(resultWrapper);
            protected override ValueTask<Coroutine<int>> Unwrap(Coroutine<int> x) => new(x);
        }

        public class ReturnAfterDelay : AbstractReturnAfterDelay<int, int>
        {
            protected override Coroutine<int> ConstantAfterDelay() => WithContext(_defaultContext, async () => {
                await Task.Delay(ContinueAfterTimeInMs).ConfigureAwait(false);
                return ExpectedResult;
            });

            protected override ValueTask<int> Unwrap(int resultWrapper) => new(resultWrapper);
            protected override ValueTask<Coroutine<int>> Unwrap(Coroutine<int> x) => new(x);

            [Fact]
            public override Task GetResult_Throws() => base.GetResult_Throws();
        }
    }
}
