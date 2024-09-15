namespace Vernuntii.Coroutines.Iterators;

public partial class AsyncIteratorTests
{
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
}
