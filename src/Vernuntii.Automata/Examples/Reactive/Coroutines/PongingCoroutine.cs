using Vernuntii.Coroutines;
using Vernuntii.Examples.Reactive.Coroutines;
using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Extensions.Coroutines;

namespace Vernuntii.Exmaples.Reactive.Coroutines;

internal class PongingCoroutine
{
    public static IEventDiscriminator<Pong> Ponged = EventDiscriminator.New<Pong>();

    public async Coroutine PongWhenPinged()
    {
        var pingedChannel = await __co.Channel(e => e.Every(PingingCoroutine.Pinged));

        while (true) {
            var pinged = await __co.Take(pingedChannel);
            Console.WriteLine(pinged);
            await __co.Emit(Ponged, new Pong(pinged.Counter + 1)).ConfigureAwait(false);
        }
    }
}
