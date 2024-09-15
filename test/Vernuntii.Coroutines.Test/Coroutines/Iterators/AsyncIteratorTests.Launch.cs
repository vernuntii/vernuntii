using Vernuntii.Coroutines.CompilerServices;

namespace Vernuntii.Coroutines.Iterators;
partial class AsyncIteratorTests
{
    public class Launch
    {
        public class ReturnSynchronously : AbstractReturnSynchronously<CoroutineAwaitable<int>, int>
        {
            protected override Coroutine<CoroutineAwaitable<int>> Constant() => Launch(() => Coroutine.FromResult(ExpectedResult));
            protected override ValueTask<int> Unwrap(CoroutineAwaitable<int> resultWrapper) => resultWrapper;
            protected override async ValueTask<Coroutine<int>> Unwrap(Coroutine<CoroutineAwaitable<int>> coroutine) => await coroutine;
        }

        public class ReturnAfterDelay : AbstractReturnAfterDelay<CoroutineAwaitable<int>, int>
        {
            protected override Coroutine<CoroutineAwaitable<int>> ConstantAfterDelay() => Launch(async () => {
                await Task.Delay(ContinueAfterTimeInMs).ConfigureAwait(false);
                return ExpectedResult;
            });

            protected override ValueTask<int> Unwrap(CoroutineAwaitable<int> resultWrapper) => resultWrapper;
            protected override async ValueTask<Coroutine<int>> Unwrap(Coroutine<CoroutineAwaitable<int>> x) => await x;

            [Fact]
            public override Task GetResult_Returns() => base.GetResult_Returns();
        }
    }
}
