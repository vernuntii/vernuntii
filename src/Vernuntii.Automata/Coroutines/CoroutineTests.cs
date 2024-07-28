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

    public static async Task HandleAsnyc()
    {
        Run(async () => {
            var task = CO1(1000);
            task.PropagateCoroutineArgument(266);
            task.StartStateMachine();
            Console.WriteLine(await task);
        });

        //// Complete the synchronization context work
        //syncContext.Complete();
        //thread.Join();
    }

    static async Coroutine<int> CO1(int number)
    {
        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        Console.WriteLine($"{nameof(CO1)} before");
        await Task.Delay(100).ConfigureAwait(false);
        await Task.Yield();
        await CO2();
        await Task.Delay(100).ConfigureAwait(false);
        Console.WriteLine($"{nameof(CO1)} after");
        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        return 500;

        async Coroutine CO2()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine($"{nameof(CO2)} before");
            await Task.Delay(100).ConfigureAwait(false);
            await Task.Yield();
            await CO3();
            await Task.Delay(100).ConfigureAwait(false);
            Console.WriteLine($"{nameof(CO2)} after");
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

            async Coroutine CO3()
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                Console.WriteLine($"{nameof(CO3)} before");
                await Task.Delay(100).ConfigureAwait(false);
                Task.Yield();
                var t = await new CoroutineInvocation<int>(ValueTask.FromResult(2), test);
                void test(in CoroutineInvocationArgumentReceiver argumentReceiver)
                {
                    ;
                }
                await Task.Delay(100).ConfigureAwait(false);
                Console.WriteLine($"{nameof(CO3)} after");
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            }
        }
    }
}
