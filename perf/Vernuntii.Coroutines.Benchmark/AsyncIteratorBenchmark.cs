using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Vernuntii.Coroutines.Benchmark
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class AsyncIteratorBenchmark
    {
        private const int RunLess = 9;
        private const int RunMore = 999;
        private const int RunMost = 99999;
        private const int Constant = int.MaxValue;

        [Benchmark]
        [Arguments(RunLess)]
        [Arguments(RunMore)]
        [Arguments(RunMost)]
        public async Task AsyncIterator(int runs)
        {
            var generator = Generator(runs).GetAsyncIterator();
            var results = new List<int>();

            while (await generator.MoveNextAsync()) {
                results.Add(((Arguments.CallArgument<int, int>)generator.Current).Closure);
            }

            static async Coroutine Generator(int runs)
            {
                var run = runs;
                while (run-- > 0) {
                    await Call(static async x => x, Constant);
                    await Task.Yield();
                }
            }
        }

        [Benchmark]
        [Arguments(RunLess)]
        [Arguments(RunMore)]
        [Arguments(RunMost)]
        public async Task AsyncEnumerable(int runs)
        {
            var generator = Generator(runs).GetAsyncEnumerator();
            var results = new List<int>();

            while (await generator.MoveNextAsync()) {
                results.Add(generator.Current);
            }

            static async IAsyncEnumerable<int> Generator(int runs)
            {
                var run = runs;
                while (run-- > 0) {
                    yield return Constant;
                    await Task.Yield();
                }
            }
        }
    }
}
