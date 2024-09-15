namespace Vernuntii.Coroutines.Iterators;
partial class AsyncIteratorTests
{
    public class Call
    {
        public static IEnumerable<object[]> NonGeneric_IsSiblingCoroutine_Generator()
        {
            yield return [new ReleativeCoroutineHolder(Call(async () => { }))];
            yield return [new ReleativeCoroutineHolder(Call(() => Call(async () => { })))];
        }

        [Theory]
        [MemberData(nameof(NonGeneric_IsSiblingCoroutine_Generator))]
        public void NonGeneric_IsSiblingCoroutine(ReleativeCoroutineHolder holder)
        {
            holder.Coroutine.CoroutineAction.Should().Be(CoroutineAction.Sibling);
        }

        public class ReturnSynchronously : AbstractReturnSynchronously<int, int>
        {
            protected override Coroutine<int> Constant() => Call(() => new Coroutine<int>(ExpectedResult));
            protected override ValueTask<int> Unwrap(int resultWrapper) => new(resultWrapper);
            protected override ValueTask<Coroutine<int>> Unwrap(Coroutine<int> coroutine) => new(coroutine);
        }

        public class ReturnAfterDelay : AbstractReturnAfterDelay<int, int>
        {
            protected override Coroutine<int> ConstantAfterDelay() => Call(async () => {
                await Task.Delay(ContinueAfterTimeInMs).ConfigureAwait(false);
                return ExpectedResult;
            });

            protected override ValueTask<int> Unwrap(int resultWrapper) => new(resultWrapper);
            protected override ValueTask<Coroutine<int>> Unwrap(Coroutine<int> x) => ValueTask.FromResult(x);

            [Fact]
            public override Task GetResult_Throws() => base.GetResult_Throws();
        }
    }
}
