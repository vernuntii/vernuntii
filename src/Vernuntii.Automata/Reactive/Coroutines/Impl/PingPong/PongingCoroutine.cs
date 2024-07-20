using Vernuntii.Reactive.Broker;
using static Vernuntii.Reactive.Coroutines.Effects;

namespace Vernuntii.Reactive.Coroutines.Impl.PingPong;

internal class PongingCoroutine
{
    public static IEventDiscriminator<Pong> Ponged = EventDiscriminator.New<Pong>();

    private readonly IEventBroker _eventBroker;

    public PongingCoroutine(IEventBroker eventBroker) =>
        _eventBroker = eventBroker;

    public async Coroutine PongWhenPinged()
    {
        var pingedTrace = await Trace(_eventBroker.Every(PingingCoroutine.Pinged));

        while (true) {
            var pinged = await Take(pingedTrace);
            Console.WriteLine(pinged);
            await _eventBroker.EmitAsync(Ponged, new Pong(pinged.Counter)).ConfigureAwait(false);
        }
    }

    public async Coroutine PongWhenPinged2()
    {
        var pingedTrace = await Trace(_eventBroker.Every(PingingCoroutine.Pinged));

        while (true) {
            var pinged = await Take(pingedTrace);
            Console.WriteLine(pinged);
            await _eventBroker.EmitAsync(Ponged, new Pong(pinged.Counter)).ConfigureAwait(false);
        }
    }
}
