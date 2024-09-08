using Vernuntii.Reactive.Broker;
using Vernuntii.Coroutines;
using System.Runtime.CompilerServices;
using Vernuntii.Reactive.Extensions.Coroutines;
using Vernuntii.Exmaples.Reactive.Coroutines;

internal class Program
{
    private static async Task Main(string[] args)
    {
        async Task CoroutineLoop(int runs = 99999 * 10)
        {
            var list = new List<int>();
            await Coroutine.Start(static x => Generator(x.runs, x.list), (runs, list));

            [MethodImpl(MethodImplOptions.NoOptimization)]
            static async Coroutine Generator(int runs, List<int> list)
            {
                var run = runs;
                while (run-- > 0) {
                    list.Add(await Call(static async x => {
                        await Task.Yield();
                        return x;
                    }, int.MaxValue));
                }
            }
        }

        async Task AsyncIterator(int runs = 99999 * 10)
        {
            var generator = Generator(runs).GetAsyncIterator();
            var results = new List<int>();

            while (await generator.MoveNextAsync()) {
                results.Add(((Arguments.ReturnArgument<int>)generator.Current).Result);
            }

            [MethodImpl(MethodImplOptions.NoOptimization)]
            static async Coroutine Generator(int runs)
            {
                var run = runs;
                while (run-- > 0) {
                    await Return(run);
                    await Task.Yield();
                }
            }
        }

        //await CoroutineLoop();
        //await AsyncIterator();
        //var pool = new FixedSizedArrayPool<int>(4);
        //var t35346 = pool.Rent();
        //t35346[0] = 4;
        //pool.Return(t35346);
        //var t35346534 = pool.Rent();
        //return;


        //var t = CoroutineScope.s_coroutineScopeKey.ToString();
        //await CoroutineTests.HandleAsnyc();
        //Environment.Exit(0);

        //await new ExampleAsync().GetAnswerAsync(0);

        var eventBroker = new EventBroker();

        var context = new CoroutineContext() {
            _keyedServicesToBequest = CoroutineContextServiceMap.CreateRange(1, eventBroker, static (map, value) => {
                map.Emplace(ServiceKeys.EventBrokerKey, value);
            })
        };

        var ponging = Coroutine.Start(new PingingCoroutine().PingWhenPonged, context);
        var pinging = Coroutine.Start(new PongingCoroutine().PongWhenPinged, context);
        await Task.Delay(100);
        await eventBroker.EmitAsync(PongingCoroutine.Ponged, new Vernuntii.Examples.Reactive.Coroutines.Pong(1));
        await Task.WhenAll(ponging.AsTask(), pinging.AsTask());
    }
}
