namespace Vernuntii.Coroutines.Iterators;

public partial class AsyncIteratorTests
{
    [Fact]
    public async Task Clone_DoesNotConsumesOriginalIterator()
    {
        const int OurResult = 1;
        const int TheirResult = 2;
        var our = AsyncIterator.Create(provider: new Func<Coroutine<int>>(async () => {
            var one = await Exchange(OurResult);
            return one;
        }), isCloneable: true);
        _ = await our.MoveNextAsync();
        var their = our.Clone();
        _ = await their.MoveNextAsync();
        their.YieldReturn(TheirResult);
        var ourResult = await our.GetResultAsync();
        var theirResult = await their.GetResultAsync();
        ourResult.Should().Be(OurResult);
        theirResult.Should().Be(TheirResult);
    }

    public abstract class AbstractReturnSynchronously<TResultWrapper, TResult>
    {
        protected const int ExpectedResult = 2;

        protected abstract Coroutine<TResultWrapper> Constant();
        protected abstract ValueTask<TResult> Unwrap(TResultWrapper resultWrapper);
        protected abstract ValueTask<Coroutine<TResult>> Unwrap(Coroutine<TResultWrapper> coroutine);

        [Fact]
        public async Task MoveNext_ReturnsFalse()
        {
            var iterator = Constant().GetAsyncIterator();
            var canMoveNext = await iterator.MoveNextAsync().ConfigureAwait(false);
            canMoveNext.Should().BeFalse();
        }

        [Fact]
        public async Task GetResult_Returns()
        {
            var iterator = Constant().GetAsyncIterator();
            var result = await Unwrap(iterator.GetResult());
            result.Should().Be(ExpectedResult);
        }

        [Fact]
        public async Task GetResultAsync_Awaits()
        {
            var iterator = Constant().GetAsyncIterator();
            var asyncResult = await Unwrap(iterator.GetResultAsync());
            var result = await asyncResult.ConfigureAwait(false);
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

    public abstract class AbstractReturnAfterDelay<TResultWrapper, TResult>
    {
        protected const int ExpectedResult = 2;

        protected abstract Coroutine<TResultWrapper> ConstantAfterDelay();
        protected abstract ValueTask<TResult> Unwrap(TResultWrapper resultWrapper);
        protected abstract ValueTask<Coroutine<TResult>> Unwrap(Coroutine<TResultWrapper> coroutine);

        [Fact]
        public async Task MoveNext_ReturnsFalse()
        {
            var iterator = ConstantAfterDelay().GetAsyncIterator();
            var canMoveNext = await iterator.MoveNextAsync().ConfigureAwait(false);
            canMoveNext.Should().Be(false);
        }

        public virtual async Task GetResult_Throws()
        {
            var iterator = ConstantAfterDelay().GetAsyncIterator();

            var result = iterator
                .Invoking(x => x.GetResult())
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*not finished yet*");
        }

        public virtual async Task GetResult_Returns()
        {
            var iterator = ConstantAfterDelay().GetAsyncIterator();
            var asyncResult = Unwrap(iterator.GetResult());
            var result = await asyncResult;
            result.Should().Be(ExpectedResult);
        }

        [Fact]
        public async Task GetResultAsync_Awaits()
        {
            var iterator = ConstantAfterDelay().GetAsyncIterator();
            var asyncResult = await Unwrap(iterator.GetResultAsync());
            var result = await asyncResult;
            result.Should().Be(ExpectedResult);
        }
    }

    public abstract class AbstractYieldReturnSynchronously
    {
        protected const int ExpectedResult = 2;

        protected abstract Coroutine<int> YieldConstant();

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
        public async Task MoveNextThenYieldReturnThenGetResult_Returns()
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
        public async Task MoveNextThenMoveNextThenGetResult_Returns()
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
