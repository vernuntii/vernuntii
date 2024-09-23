using Vernuntii.Coroutines.Iterators;
using static Vernuntii.Coroutines.Iterators.Yielders;

try {
    const int OurResult = 1;
    const int TheirResult = 2;
    var our = AsyncIterator.Create(async () => { var one = await Exchange(OurResult); return one; }, isCloneable: true);
    _ = await our.MoveNextAsync();
    var their = our.Clone();
    _ = await their.MoveNextAsync();
    their.YieldReturn(TheirResult);
    var ourResult = await our.GetResultAsync();
    var theirResult = await their.GetResultAsync();
} catch (Exception error) {
    Console.WriteLine(error); ;
}
