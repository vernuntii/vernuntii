namespace Vernuntii.Coroutines.Iterators;

partial class AsyncIteratorTests
{
    public class NonRelative
    {
        public class ReturnSynchronously : AbstractReturnSynchronously<int, int> {
            protected override Coroutine<int> Constant() => Coroutine.FromResult(ExpectedResult);
            protected override ValueTask<int> Unwrap(int resultWrapper) => new(resultWrapper);
            protected override ValueTask<Coroutine<int>> Unwrap(Coroutine<int> x) => new(x);
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

        public class YieldReturnSynchronously
        {
            private const int ExpectedResult = 2;

            private async Coroutine<int> YieldConstant() => await Call(async x => x, ExpectedResult);

            [Fact]
            public async Task MoveNext_ReturnsTrue()
            {
                var iterator = YieldConstant().GetAsyncIterator();
                var canMoveNext = await iterator.MoveNextAsync().ConfigureAwait(false);
                canMoveNext.Should().BeTrue();
            }

            [Fact]
            public async Task MoveNextThenMoveNext_ReturnsFalse()
            {
                var iterator = YieldConstant().GetAsyncIterator();
                _ = await iterator.MoveNextAsync().ConfigureAwait(false);
                var canMoveNext = await iterator.MoveNextAsync().ConfigureAwait(false);
                canMoveNext.Should().BeFalse();
            }

            [Fact]
            public async Task GetResult_Throws()
            {
                var iterator = YieldConstant().GetAsyncIterator();

                var result = iterator
                    .Invoking(x => x.GetResult())
                    .Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("*not finished yet*");
            }

            [Fact]
            public async Task GetResultAsync_Awaits()
            {
                var iterator = AsyncIterator.Create(YieldConstant());
                var result = await iterator.GetResultAsync();
                result.Should().Be(ExpectedResult);
            }

            [Fact]
            public async Task Throw_Fails()
            {
                var iterator = AsyncIterator.Create(YieldConstant);
                iterator
                    .Invoking(x => x.Throw(new Exception1()))
                    .Should()
                    .ThrowExactly<InvalidOperationException>()
                    .WithMessage("*not started*already finished*not suspended*");
            }

            [Fact]
            public async Task MoveNextThenYieldReturnThenThenGetResult_Returns()
            {
                const int expectedYieldResult = ExpectedResult + 1;
                var iterator = YieldConstant().GetAsyncIterator();
                _ = await iterator.MoveNextAsync().ConfigureAwait(false);
                iterator.YieldReturn(expectedYieldResult);
                var asyncResult = iterator.GetResultAsync();
                var result = await asyncResult;
                result.Should().Be(expectedYieldResult);
            }

            [Fact]
            public async Task MoveNextThenMoveNextThenThenGetResult_Returns()
            {
                var iterator = YieldConstant().GetAsyncIterator();
                _ = await iterator.MoveNextAsync().ConfigureAwait(false);
                _ = await iterator.MoveNextAsync().ConfigureAwait(false);
                var result = iterator.GetResult();
                result.Should().Be(ExpectedResult);
            }

            [Fact]
            public async Task MoveNextThenGetResultAsync_Awaits()
            {
                var iterator = AsyncIterator.Create(YieldConstant());
                _ = await iterator.MoveNextAsync().ConfigureAwait(false);
                var result = await iterator.GetResultAsync();
                result.Should().Be(ExpectedResult);
            }

            [Fact]
            public async Task MoveNextThenThrow_Succeeds()
            {
                var iterator = YieldConstant().GetAsyncIterator();
                _ = await iterator.MoveNextAsync().ConfigureAwait(false);
                iterator.Throw(new Exception1());
                await iterator.Awaiting(new Func<IAsyncIterator<int>, Task>(async x => await x.GetResultAsync()))
                    .Should()
                    .ThrowExactlyAsync<Exception1>().ConfigureAwait(false);
            }
        }
    }
}
