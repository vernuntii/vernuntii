using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Coroutines.Steps;

namespace Vernuntii.Reactive.Coroutines.PingPong;

internal class PingCoroutine : ICoroutines
{
    public static IEventDiscriminator<Ping> Pinged = EventDiscriminator.New<Ping>();

    private readonly IEventBroker _eventStore;

    public PingCoroutine(IEventBroker eventStore) =>
        _eventStore = eventStore;

    public async IAsyncEnumerable<IStep> PongWhenPinged()
    {
        yield return this.Trace(_eventStore.Every(Pinged), out var pingedTrace);

        while (true)
        {
            //test.expect
            yield return this.Take(pingedTrace, out var pinged);
            Console.WriteLine(pinged.Value);
            await _eventStore.EmitAsync(PongCoroutine.Ponged, new Pong(pinged.Value.Counter));
        }
    }

    public Coroutine PongWhenPinged2() => new Coroutine(async dispatcher =>
    {
        
    });

    //    private IStep test() {
    //        return null!;

    //#pragma warning disable CS0162 // Unreachable code detected
    //#pragma warning disable IDE0035 // Unreachable code detected
    //        return null!;
    //#pragma warning restore CS0162 // Unreachable code detected
    //    }

    //class tst : Attribute
}

class Coroutine
{
    public Coroutine(Func<ICoroutineDispatcher, Task> definition)
    {
    }
}

public interface ICoroutineDispatcher
{
    
}
