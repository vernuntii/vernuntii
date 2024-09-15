using Vernuntii.Coroutines.CompilerServices;

namespace Vernuntii.Coroutines.Iterators;
partial class AsyncIteratorTests
{
    public class Spawn
    {
        public static IEnumerable<object[]> NonGeneric_IsSibling_Generator()
        {
            yield return [new ReleativeCoroutineHolder(Spawn(async () => { }))];
            yield return [new ReleativeCoroutineHolder(Spawn(() => Call(async () => { })))];
        }

        [Theory]
        [MemberData(nameof(NonGeneric_IsSibling_Generator))]
        public void NonGeneric_IsSibling(ReleativeCoroutineHolder holder)
        {
            holder.Coroutine.CoroutineAction.Should().Be(CoroutineAction.Sibling);
        }

        public class ReturnSynchronously : AbstractReturnSynchronously<CoroutineAwaitable<int>, int>
        {
            protected override Coroutine<CoroutineAwaitable<int>> Constant() => Spawn(() => Coroutine.FromResult(ExpectedResult));
            protected override ValueTask<int> Unwrap(CoroutineAwaitable<int> resultWrapper) => resultWrapper;
            protected override async ValueTask<Coroutine<int>> Unwrap(Coroutine<CoroutineAwaitable<int>> coroutine) => await coroutine;
        }

        public class ReturnAfterDelay : AbstractReturnAfterDelay<CoroutineAwaitable<int>, int>
        {
            protected override Coroutine<CoroutineAwaitable<int>> ConstantAfterDelay() => Spawn(async () => {
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
