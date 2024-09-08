using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Coroutines;
using Vernuntii.Reactive.Coroutines.PingPong;
using Vernuntii.Reactive.Coroutines.AsyncEffects;
using Vernuntii.Coroutines;
using System.Runtime.CompilerServices;

internal class Program
{
    private static async Task Main(string[] args)
    {
        async Task CoroutineLoop(int runs = 99999*10)
        {
            var list = new List<int>();
            await Vernuntii.Coroutines.Coroutine.Start(static x => Generator(x.runs, x.list), (runs, list));

            [MethodImpl(MethodImplOptions.NoOptimization)]
            static async Vernuntii.Coroutines.Coroutine Generator(int runs, List<int> list)
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

        async Task AsyncIterator(int runs = 99999*10)
        {
            var generator = Generator(runs).GetAsyncIterator();
            var results = new List<int>();

            while (await ((Vernuntii.Coroutines.Iterators.IAsyncIterator)generator).MoveNextAsync()) {
                results.Add(((Arguments.ReturnArgument<int>)((Vernuntii.Coroutines.Iterators.IAsyncIterator)generator).Current).Result);
            }

            [MethodImpl(MethodImplOptions.NoOptimization)]
            static async Vernuntii.Coroutines.Coroutine Generator(int runs)
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
        var pool = new FixedSizedArrayPool<int>(4);
        var t35346 = pool.Rent();
        t35346[0] = 4;
        pool.Return(t35346);
        var t35346534 = pool.Rent();
        return;


        var t = CoroutineScope.s_coroutineScopeKey.ToString();
        await CoroutineTests.HandleAsnyc();
        Environment.Exit(0);

        //await new ExampleAsync().GetAnswerAsync(0);

        var eventBroker = new EventBroker();

        //var pongCoroutine = new PongCoroutine(eventBroker);
        //while (true) {
        //    if (Interlocked.CompareExchange(ref CoroutineMethodBuilder.s_locker, 1, 0) == 0) {
        //        CoroutineMethodBuilder.s_site = new();
        //        await Test2Async();
        //        break;
        //    }
        //}

        var coroutineExecutor = new CoroutineExecutorBuilder()
            .AddStepStore(new EffectStore())
            .Build();

        coroutineExecutor.Start(new PongingCoroutine(eventBroker).PongWhenPinged);
        coroutineExecutor.Start(new PingingCoroutine(eventBroker).PingWhenPonged);
        await Task.Delay(500);
        await eventBroker.EmitAsync(PingingCoroutine.Pinged, new Vernuntii.Reactive.Coroutines.PingPong.Ping(1));
        await coroutineExecutor.WhenAll();

        //async Coroutine TestAsync()
        //{
        //    await Test2Async();
        //}


        //async Coroutine Test2Async()
        //{
        //    //var pingedTrace = await pongCoroutine.Trace(eventBroker.Every(Pinged));
        //    //;
        //    //await Task.Yield();
        //    //await Task.Delay(99999);
        //    //await Task.Yield();
        //}
    }
}
