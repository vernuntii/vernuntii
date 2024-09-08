using Vernuntii.Coroutines;
using Vernuntii.Examples.Reactive.Coroutines;
using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Extensions.Coroutines;

namespace Vernuntii.Exmaples.Reactive.Coroutines;

internal class PingingCoroutine
{
    public static IEventDiscriminator<Pong> Pinged = EventDiscriminator.New<Pong>();

    public async Coroutine PingWhenPonged()
    {
        var pongedChannel = await __co.Channel(e => e.Every(PongingCoroutine.Ponged));

        while (true) {
            var ponged = await __co.Take(pongedChannel);
            Console.WriteLine(ponged);
            await __co.Emit(Pinged, new Pong(ponged.Counter)).ConfigureAwait(false);
        }
    }
}
