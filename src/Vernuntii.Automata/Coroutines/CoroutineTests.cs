using System.Collections.Concurrent;

namespace Vernuntii.Coroutines;

public static class CoroutineTests
{
    // Custom SynchronizationContext to force continuations on a single thread
    public class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<(SendOrPostCallback, object?)> _queue =
            new BlockingCollection<(SendOrPostCallback, object?)>();

        public override void Post(SendOrPostCallback d, object? state)
        {
            _queue.Add((d, state));
        }

        public void RunOnCurrentThread()
        {
            while (_queue.TryTake(out var workItem, Timeout.Infinite)) {
                workItem.Item1(workItem.Item2);
            }
        }

        public void Complete() => _queue.CompleteAdding();
    }

    public static void Run(Func<Task> func)
    {
        var prevCtx = SynchronizationContext.Current;

        try {
            var syncCtx = new SingleThreadSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(syncCtx);
            var t = func();
            t.ContinueWith(delegate { syncCtx.Complete(); }, TaskScheduler.Default);

            syncCtx.RunOnCurrentThread();
            t.GetAwaiter().GetResult();
        } finally { SynchronizationContext.SetSynchronizationContext(prevCtx); }
    }

    const int waitTime = 1;

    public static async Task HandleAsnyc()
    {
        Run(async () => {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            var task = CO1(1000);
            var node = new CoroutineStackNode(new CoroutineContext());
            task.PropagateCoroutineNode(ref node);
            task.StartStateMachine();
            Console.WriteLine(await task);
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        });

        //// Complete the synchronization context work
        //syncContext.Complete();
        //thread.Join();
    }

    static async Coroutine<int> CO1(int number)
    {
        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        Console.WriteLine($"{nameof(CO1)} before");
        await Task.Delay(waitTime).ConfigureAwait(false);
        await Task.Yield();
        await CO2();
        await Task.Delay(waitTime).ConfigureAwait(false);
        Console.WriteLine($"{nameof(CO1)} after");
        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        return 500;

        async Coroutine CO2()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine($"{nameof(CO2)} before");
            await Task.Delay(waitTime).ConfigureAwait(false);
            await Task.Yield();
            await CO3();
            await Task.Delay(waitTime).ConfigureAwait(false);
            Console.WriteLine($"{nameof(CO2)} after");
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

            async Coroutine CO3()
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                Console.WriteLine($"{nameof(CO3)} before");
                await Task.Delay(waitTime).ConfigureAwait(false);
                Task.Yield();
                var t2 = new TaskCompletionSource<int>();
                Task.Run(async () => {
                    await Task.Delay(100);
                    t2.SetResult(2);
                }, CancellationToken.None);
                var t = await new Coroutine<int>(new ValueTask<int>(t2.Task), test);
                void test(in CoroutineArgumentReceiver argumentReceiver)
                {
                    argumentReceiver.ReceiveArgument("hello from coroutine");
                }
                await Task.Delay(waitTime).ConfigureAwait(false);
                Console.WriteLine($"{nameof(CO3)} after");
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            }
        }
    }
}
