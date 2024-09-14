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

        public class ReturnSynchronously
        {
            private const int ExpectedResult = 2;

            private Coroutine<CoroutineAwaitable<int>> Constant() => Spawn(() => Coroutine.FromResult(2));

            [Fact]
            public async Task MoveNext_ReturnsFalse()
            {
                var iterator = AsyncIterator.Create(Constant());
                var canMoveNext = await iterator.MoveNextAsync().ConfigureAwait(false);
                canMoveNext.Should().BeFalse();
            }

            [Fact]
            public async Task GetResult_Returns()
            {
                var iterator = AsyncIterator.Create(Constant());
                var asyncResult = iterator.GetResult();
                var result = await asyncResult;
                result.Should().Be(ExpectedResult);
            }

            [Fact]
            public async Task GetResultAsync_Awaits()
            {
                var iterator = AsyncIterator.Create(Constant());
                var asyncResult = await iterator.GetResultAsync().ConfigureAwait(false);
                var result = await asyncResult;
                result.Should().Be(ExpectedResult);
            }

            [Fact]
            public async Task Throw_Fails()
            {
                var iterator = AsyncIterator.Create(Constant);
                iterator
                    .Invoking(x => x.Throw(new Exception1()))
                    .Should()
                    .ThrowExactly<InvalidOperationException>()
                    .WithMessage("*not started*already finished*not suspended*");
            }
        }

        public class ReturnAfterDelay
        {
            private const int ExpectedResult = 2;

            private Coroutine<CoroutineAwaitable<int>> ConstantAfterDelay() => Spawn(async () => {
                await Task.Delay(ContinueAfterTimeInMs).ConfigureAwait(false);
                return ExpectedResult;
            });

            [Fact]
            public async Task MoveNext_ReturnsFalse()
            {
                var iterator = ConstantAfterDelay().GetAsyncIterator();
                var canMoveNext = await iterator.MoveNextAsync().ConfigureAwait(false);
                canMoveNext.Should().Be(false);
            }

            [Fact]
            public async Task GetResult_Returns()
            {
                var iterator = ConstantAfterDelay().GetAsyncIterator();
                var asyncResult = iterator.GetResult();
                var result = await asyncResult;
                result.Should().Be(ExpectedResult);
            }

            [Fact]
            public async Task GetResultAsync_Awaits()
            {
                var iterator = ConstantAfterDelay().GetAsyncIterator();
                var asyncResult = await iterator.GetResultAsync();
                var result = await asyncResult;
                result.Should().Be(ExpectedResult);
            }
        }
    }
}
