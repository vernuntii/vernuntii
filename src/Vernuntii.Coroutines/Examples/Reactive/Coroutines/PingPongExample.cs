using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Extensions.Coroutines;

namespace Vernuntii.Examples.Reactive.Coroutines;

internal class PingPongExample
{
    public static IEventDiscriminator<Pong> Pinged = EventDiscriminator.New<Pong>();
    public static IEventDiscriminator<Pong> Ponged = EventDiscriminator.New<Pong>();

    public async Task PingPongAsync()
    {
        var eventBroker = new EventBroker();

        var context = new CoroutineContext() {
            _keyedServicesToBequest = CoroutineContextServiceMap.CreateRange(length: 1, eventBroker, static (map, value) => {
                map.Emplace(ServiceKeys.EventBrokerKey, value);
            })
        };

        var ponging = Coroutine.Start(PingWhenPonged, context);
        var pinging = Coroutine.Start(PongWhenPinged, context);
        await eventBroker.EmitAsync(Ponged, new Pong(1));
        await Task.WhenAll(ponging.AsTask(), pinging.AsTask());
    }

    public static async Coroutine PingWhenPonged()
    {
        using var pongedChannel = await Channel(broker => broker.Every(Ponged));

        while (true) {
            var ponged = await Take(pongedChannel);
            Console.WriteLine(ponged);
            await Emit(Pinged, new Pong(ponged.Counter)).ConfigureAwait(false);
        }
    }

    public static async Coroutine PongWhenPinged()
    {
        using var pingedChannel = await Channel(broker => broker.Every(Pinged));

        while (true) {
            var pinged = await Take(pingedChannel);
            Console.WriteLine(pinged);
            await Emit(Ponged, new Pong(pinged.Counter + 1)).ConfigureAwait(false);
        }
    }

    public record Pong(int Counter);
    public record Ping(int Counter);
}
