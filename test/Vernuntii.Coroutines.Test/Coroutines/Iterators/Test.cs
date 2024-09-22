namespace Vernuntii.Coroutines.Iterators;

public class Test
{
public interface IAsyncIterator<TResult>
{
    bool IsCloneable { get; }
    object Current { get; }
    ValueTask<bool> MoveNextAsync();
    void YieldReturn<TYieldResult>(TYieldResult result);
    void Return(TResult result);
    void Throw(Exception e);
    TResult GetResult();
    Coroutine<TResult> GetResultAsync();
    IAsyncIterator<TResult> Clone();
}

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

class Test2
{
    public static test2 create(ref test1 test1)
    {
        var test2 = new test2();
        test2.gargong = test1.blubb;
        test2.test3 = test1.test3;
        return test2;
    }

    public struct test1
    {
        public int blubb;
        public int test3;
    }

    public struct test2
    {
        public int gargong;
        public int test3;
    }

    public class test3
    {
        public int three;
    }
}
