using Vernuntii.Reactive.Broker;
using static Vernuntii.Reactive.Coroutines.AsyncEffects.Effects;

namespace Vernuntii.Reactive.Coroutines.PingPong;

internal partial class PongingCoroutine
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


            var test = await Call(() => Task.FromResult(1));

            var test = await All(new {
                first = Task.FromResult(1),
                second = Take(pingedTrace)
            }, Goofy);


            Console.WriteLine(pinged);
            await _eventBroker.EmitAsync(Ponged, new Pong(pinged.Counter)).ConfigureAwait(false);
        }
    }
}
