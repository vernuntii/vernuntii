using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Coroutines.Steps;

namespace Vernuntii.Reactive.Coroutines.PingPong;

internal class PingCoroutine : ICoroutineDefinition
{
    public static IEventDiscriminator<Ping> Pinged = EventDiscriminator.New<Ping>();

    private readonly IEventBroker _eventStore;

    public PingCoroutine(IEventBroker eventStore) =>
        _eventStore = eventStore;

    public async IAsyncEnumerable<IStep> PongWhenPinged()
    {
        yield return this.Trace(_eventStore.Every(Pinged), out var pingedTrace);

        while (true) {
            yield return this.Take(pingedTrace, out var pinged);
            Console.WriteLine(pinged.Value);
            await _eventStore.EmitAsync(PongCoroutine.Ponged, new Pong(pinged.Value.Counter));
        }
    }
}
