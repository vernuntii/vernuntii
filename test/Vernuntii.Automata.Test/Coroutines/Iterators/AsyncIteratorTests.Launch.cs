namespace Vernuntii.Coroutines.Iterators;
partial class AsyncIteratorTests
{
    public class Launch
    {
        public class Synchronous
        {
            private const int ExpectedResult = 2;

            private Coroutine<Coroutine<int>> LaunchThenReturn() => Launch(() => Coroutine.FromResult(2));

            [Fact]
            public async Task MoveNext_ReturnsFalse()
            {
                var iterator = AsyncIterator.Create(LaunchThenReturn());
                var canMoveNext = await iterator.MoveNextAsync().ConfigureAwait(false);
                canMoveNext.Should().BeFalse();
            }

            [Fact]
            public async Task GetResult_Returns()
            {
                var iterator = AsyncIterator.Create(LaunchThenReturn());
                var asyncResult = iterator.GetResult();
                var result = await asyncResult;
                result.Should().Be(ExpectedResult);
            }

            [Fact]
            public async Task GetResultAsync_Awaits()
            {
                var iterator = AsyncIterator.Create(LaunchThenReturn());
                var asyncResult = await iterator.GetResultAsync().ConfigureAwait(false);
                var result = await asyncResult;
                result.Should().Be(ExpectedResult);
            }
        }

        public class Asynchronous
        {
            private const int ExpectedResult = 2;

            private async Coroutine<int> ReturnAfterDelay()
            {
                await Task.Delay(ContinueAfterTimeInMs).ConfigureAwait(false);
                return ExpectedResult;
            }

            [Fact]
            public async Task MoveNext_ReturnsFalse()
            {
                var iterator = AsyncIterator.Create(ReturnAfterDelay());
                var canMoveNext = await iterator.MoveNextAsync().ConfigureAwait(false);
                canMoveNext.Should().Be(false);
            }

            [Fact]
            public async Task GetResult_Throws()
            {
                var iterator = AsyncIterator.Create(ReturnAfterDelay());

                var result = iterator
                    .Invoking(x => x.GetResult())
                    .Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("*not finished yet*");
            }

            [Fact]
            public async Task GetResultAsync_Awaits()
            {
                var iterator = AsyncIterator.Create(ReturnAfterDelay());
                var result = await iterator.GetResultAsync();
                result.Should().Be(ExpectedResult);
            }
        }
    }
}
