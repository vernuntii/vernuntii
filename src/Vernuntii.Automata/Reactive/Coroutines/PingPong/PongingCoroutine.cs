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

            var test = await All<Goofy>(new {
                first = Task.FromResult(""),
                second = Take(pingedTrace)
            });

            //var blubb = await All<Goofy>(new {
            //    first = Task.FromResult(""),
            //    second = Take(pingedTrace)
            //});

            Console.WriteLine(pinged);
            await _eventBroker.EmitAsync(Ponged, new Pong(pinged.Counter)).ConfigureAwait(false);
        }
    }
}

//partial interface IBlub
//{
//    partial class Hahaha
//    {
//        partial record blubb
//        {
//            partial struct test
//            {
//                partial class Test
//                {
//                    public async Coroutine PongWhenPinged()
//                    {
//                        while (true) {
//                            var test = await All<Goofy>(new {
//                                first = Call(() => Task.FromResult(1)),
//                                second = "hello"
//                            });

//                            var test2 = test.first;
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
