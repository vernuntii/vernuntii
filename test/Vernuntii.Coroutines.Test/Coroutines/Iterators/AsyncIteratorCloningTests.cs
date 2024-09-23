namespace Vernuntii.Coroutines.Iterators;

public class AsyncIteratorCloningTests
{
    [Fact]
    public async Task AsyncIterator_Clone_DoesNotConsumesOriginalIterator()
    {
        const int OurResult = 1;
        const int TheirResult = 2;
        var our = AsyncIterator.Create(provider: new Func<Coroutine<int>>(async () => {
            var one = await Iterators.Yielders.Exchange(OurResult);
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
}
