using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Vernuntii.Coroutines.Benchmark
{
    [MemoryDiagnoser]
    public class CoroutineLaunchTaskDelayBenchmark
    {
        [Benchmark]
        public async Task AwaitTaskDelay()
        {
            await new Func<ValueTask>(static async () => {
                var launch = new Func<ValueTask>(static async () => {
                    await Task.Delay(5).ConfigureAwait(false);
                })();

                await launch; // Explicit
            })().ConfigureAwait(false);
        }

        [Benchmark]
        public async Task AwaitCoroutineTaskDelay()
        {
            await Coroutine.Start(static async () => {
                _ = await Launch(static async () => {
                    await Task.Delay(5).ConfigureAwait(false);
                });
            }).ConfigureAwait(false);
        }
    }
}
