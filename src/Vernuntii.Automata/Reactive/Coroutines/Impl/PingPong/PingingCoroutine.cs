using System.Linq.Expressions;
using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Coroutines.Stepping;

namespace Vernuntii.Reactive.Coroutines.Impl.PingPong;

internal class PingingCoroutine : ICoroutines
{
    public static IEventDiscriminator<Ping> Pinged = EventDiscriminator.New<Ping>();

    private readonly IEventBroker _eventBroker;

    public PingingCoroutine(IEventBroker eventBroker) =>
        _eventBroker = eventBroker;

    internal async IAsyncEnumerable<IStep> PingWhenPonged()
    {
        var (test,test2) =  (await Task.FromResult(1), await Task.FromResult(2));

        yield return this.Trace(_eventBroker.Every(PongingCoroutine.Ponged), out var pongedTrace);

        while (true) {
            yield return this.Take(pongedTrace, out var ponged);
            Console.WriteLine(ponged.Value);
            await _eventBroker.EmitAsync(Pinged, new Ping(ponged.Value.Counter + 1)).ConfigureAwait(false);
        }
    }
}
