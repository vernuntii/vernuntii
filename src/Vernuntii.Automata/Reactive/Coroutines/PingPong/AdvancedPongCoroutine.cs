//using Vernuntii.Reactive.Broker;
//using Vernuntii.Reactive.Coroutines.Steps;
//using static Vernuntii.Reactive.Coroutines.Steps.Step;

//namespace Vernuntii.Reactive.Coroutines.PingPong;

//internal class AdvancedPongCoroutine
//{
//    public static IEventDiscriminator<Pong> Ponged = EventDiscriminator.New<Pong>();

//    private readonly IEventBroker _eventStore;

//    public AdvancedPongCoroutine(IEventBroker eventStore) =>
//        _eventStore = eventStore;

//    internal void CoroutineDefinition(CoroutineContext context)
//    {
//        var pongedTrace = context.Trace(_eventStore.Every(Ponged));
//        context.Spawn(PingWhenPonged);

//        async IAsyncEnumerable<IStep> PingWhenPonged()
//        {
//            while (true) {
//                async IAsyncEnumerable<IStep> _()
//                {
//                    yield return Take(pongedTrace, out var ponged);
//                    Console.WriteLine("Ponged: " + ponged.Value);
//                    await _eventStore.EmitAsync(PingCoroutine.Pinged, new Ping(ponged.Value.Counter + 1));
//                }

//                yield return Try(_)
//                    .Catch();
//            }
//        }
//    }
//}
