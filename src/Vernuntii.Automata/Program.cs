using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Coroutines;
using Vernuntii.Reactive.Coroutines.PingPong;
using Vernuntii.Reactive.Coroutines.AsyncEffects;
using Vernuntii.Coroutines;
using System.Runtime.CompilerServices;
using System.Diagnostics;

//public class TestClass
//{
//    // Method marked with DebuggerStepThrough
//    [DebuggerStepThrough]
//    public void StepThroughMethod()
//    {
//        // This will be stepped over
//        FirstSubMethod();

//        // This will also be stepped over, despite any nested calls
//        SecondSubMethod();
//    }

//    // Sub-method that you want to step into
//    [DebuggerNonUserCode] // This attribute reverts the DebuggerStepThrough effect
//    public void FirstSubMethod()
//    {
//        // When stepping through, the debugger will step into this method
//        Console.WriteLine("Inside FirstSubMethod. Debugger will step into this method.");
//    }

//    public void SecondSubMethod()
//    {
//        // Default debugging behavior
//        // When stepping through, the debugger will step into this method
//        Console.WriteLine("Inside SecondSubMethod. Debugger will step into this method.");
//    }
//}

//class Program
//{
//    static void Main()
//    {
//        var test = new TestClass();
//        test.StepThroughMethod();
//    }
//}

internal class Program
{
    private static async Task Main(string[] args)
    {
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
