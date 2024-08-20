using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Vernuntii.Coroutines.Benchmark
{
    public class CoroutineLaunchTaskDelayBenchmark
    {
        [Benchmark]
        public async Task AwaitTaskDelay()
        {
            await Task.Run(async () => {
                var launch = Task.Run(async () => {
                    await Task.Delay(5).ConfigureAwait(false);
                });

                await launch; // Explicit
            }).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task AwaitCoroutineTaskDelay()
        {
            await Coroutine.Start(async () => {
                _ = await Launch(async () => {
                    await Task.Delay(5).ConfigureAwait(false);
                });
            }).ConfigureAwait(false);
        }
    }
}
