using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Coroutines.PingPong;
using Vernuntii.Reactive.Extensions.Coroutines;

namespace Vernuntii.Exmaples.Reactive.Coroutines;

internal class PongingCoroutine
{
    public async Coroutine PongWhenPinged()
    {
        var pingedChannel = await __co.Channel(e => e.Every(PingingCoroutine.Pinged));

        while (true) {
            var pinged = await __co.Take(pingedChannel);
            Console.WriteLine(pinged);
            await __co.Emit(Vernuntii.Reactive.Coroutines.PingPong.PongingCoroutine.Ponged, new Pong(pinged.Counter)).ConfigureAwait(false);
        }
    }
}
