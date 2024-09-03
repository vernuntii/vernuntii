using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public async Task AsyncIterator(int runs)
        {
            var generator = Generator(runs).GetAsyncIterator();

            while (await generator.MoveNextAsync()) {
                //_ =  ((Closure<int>)((Arguments.CallArgument<int>)generator.Current).ProviderClosure).Value1;
                _ = generator.Current;
            }

            [MethodImpl(MethodImplOptions.NoOptimization)]
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
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public async Task AsyncEnumerable(int runs)
        {
            var generator = Generator(runs).GetAsyncEnumerator();

            while (await generator.MoveNextAsync()) {
                _ = generator.Current;
            }

            [MethodImpl(MethodImplOptions.NoOptimization)]
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
