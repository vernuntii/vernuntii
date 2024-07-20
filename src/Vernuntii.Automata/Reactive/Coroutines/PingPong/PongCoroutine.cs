using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Coroutines.Stepping;
using static Vernuntii.Reactive.Coroutines.Stepping.EventSteps;

namespace Vernuntii.Reactive.Coroutines.PingPong;

internal class PongCoroutine : ICoroutines
{
    public static IEventDiscriminator<Pong> Ponged = EventDiscriminator.New<Pong>();

    private readonly IEventBroker _eventStore;

    public PongCoroutine(IEventBroker eventStore) =>
        _eventStore = eventStore;

    internal async IAsyncEnumerable<IStep> PingWhenPonged()
    {
        yield return this.Trace(_eventStore.Every(Ponged), out var pongedTrace);

        while (true) {
            yield return this.Take(pongedTrace, out var ponged);
            Console.WriteLine(ponged.Value);
            await _eventStore.EmitAsync(PingCoroutine.Pinged, new Ping(ponged.Value.Counter + 1));
        }
    }
}
