using System.Collections.Concurrent;
using static Vernuntii.Coroutines.Effects;

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
            try {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                var task = F2(1000);
                var context = new CoroutineContext();
                var node = new CoroutineStackNode(context);
                task.PropagateCoroutineNode(ref node);
                task.StartStateMachine();
                Console.WriteLine(await task);
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            } catch (Exception error) {
                Console.WriteLine(error);
            }
        });

        //// Complete the synchronization context work
        //syncContext.Complete();
        //thread.Join();
    }

    static async Coroutine<int> N8(int _)
    {
        var t1 = LaunchAsync(async () => {
            await Task.Yield();
            await Task.Delay(500);
            Console.WriteLine("Works");
            await Task.Delay(1000);
            Console.WriteLine("FINISHED");
        });
        await t1;
        Console.WriteLine("IMMEDIATELLY RETURN");
        return 8;
    }

    static async Coroutine<int> F2(int _)
    {
        var t1 = await LaunchAsync(async () => {
            await LaunchAsync(async () => {
                await Task.Delay(2000);
                Console.WriteLine("2000");
                throw new Exception("Hello from fork");
            });

            await Task.Delay(500);
            Console.WriteLine("Works");
            await Task.Delay(1000);
            Console.WriteLine("FINISHED");
        });
        var t2 = await LaunchAsync(async () => {
            await Task.Delay(1000);
            Console.WriteLine("Works#2");
            await Task.Delay(1000);
            Console.WriteLine("FINISHED#2");
            return 13;
        });
        await Task.Delay(750);
        Console.WriteLine("IMMEDIATELLY RETURN");
        await t1;
        return 6;
    }

    static async Coroutine<int> F1(int _)
    {
        var t1 = await SpawnAsync(async () => {
            var t8 = await SpawnAsync(async () => {
                await Task.Delay(1500);
                Console.WriteLine("Works");
                await Task.Delay(3000);
                Console.WriteLine("FINISHED");
            });

            await t8;
        });
        await t1;
        var tt = await SpawnAsync(new Func<Coroutine>(async () => {
            await Task.Delay(1500);
            Console.WriteLine("Works");
            await Task.Delay(3000);
            Console.WriteLine("FINISHED");
        }));
        await tt;
        return 2;
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
                    //argumentReceiver.ReceiveArgument("hello from coroutine");
                }
                await await SpawnAsync(new Func<Coroutine>(async () => { Console.WriteLine("Hello from spawn"); }));
                await Task.Delay(waitTime).ConfigureAwait(false);
                Console.WriteLine($"{nameof(CO3)} after");
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            }
        }
    }
}
