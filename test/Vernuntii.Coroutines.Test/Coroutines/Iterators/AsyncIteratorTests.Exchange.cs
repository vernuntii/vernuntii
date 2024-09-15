namespace Vernuntii.Coroutines.Iterators;

partial class AsyncIteratorTests
{
    public class Exchange
    {
        public class ReturnSynchronously : AbstractReturnSynchronously<int, int>
        {
            protected override Coroutine<int> Constant() => Exchange(ExpectedResult);
            protected override ValueTask<int> Unwrap(int resultWrapper) => new(resultWrapper);
            protected override ValueTask<Coroutine<int>> Unwrap(Coroutine<int> coroutine) => new(coroutine);
        }

        public class YieldReturnSynchronously : AbstractYieldReturnSynchronously
        {
            protected override async Coroutine<int> YieldConstant() => await Exchange(ExpectedResult);
        }
    }
}
