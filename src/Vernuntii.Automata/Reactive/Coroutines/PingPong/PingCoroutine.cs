using System.Runtime.CompilerServices;
using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Coroutines.Stepping;

namespace Vernuntii.Reactive.Coroutines.PingPong;

internal class PingCoroutine : ICoroutines
{
    public static IEventDiscriminator<Ping> Pinged = EventDiscriminator.New<Ping>();

    private readonly IEventBroker _eventbroker;

    public PingCoroutine(IEventBroker eventBroker) =>
        _eventbroker = eventBroker;

    public async IAsyncEnumerable<IStep> PongWhenPinged()
    {
        yield return this.Trace(_eventbroker.Every(Pinged), out var pingedTrace);

        while (true)
        {
            yield return this.Take(pingedTrace, out var pinged);
            Console.WriteLine(pinged.Value);
            await _eventbroker.EmitAsync(PongCoroutine.Ponged, new Pong(pinged.Value.Counter));
        }
    }
}
