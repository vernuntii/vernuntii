using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Coroutines;
using Vernuntii.Reactive.Coroutines.PingPong;
using Vernuntii.Reactive.Coroutines.AsyncEffects;
using Vernuntii.Coroutines;

internal class Program
{
    private static async Task Main(string[] args)
    {
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
