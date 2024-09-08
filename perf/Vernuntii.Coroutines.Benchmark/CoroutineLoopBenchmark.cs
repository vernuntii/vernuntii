using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Vernuntii.Coroutines.Benchmark
{
    [MemoryDiagnoser]
    public class CoroutineLoopBenchmark
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
        public async Task CoroutineLoop(int runs)
        {
            var list = new List<int>();
            await Coroutine.Start(static x => Generator(x.runs, x.list), (runs, list));

            [MethodImpl(MethodImplOptions.NoOptimization)]
            static async Coroutine Generator(int runs, List<int> list)
            {
                var run = runs;
                while (run-- > 0) {
                    list.Add(await Call(static async x => {
                        await Task.Yield();
                        return x;
                    }, Constant));
                }
            }
        }

        [Benchmark]
        [Arguments(RunLess)]
        [Arguments(RunMore)]
        [Arguments(RunMost)]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public async Task ValueTaskLoop(int runs)
        {
            var list = new List<int>();
            await Generator(runs, list);

            [MethodImpl(MethodImplOptions.NoOptimization)]
            static async Task Generator(int runs, List<int> list)
            {
                var run = runs;
                while (run-- > 0) {
                    list.Add(await new Func<ValueTask<int>>(static async () => {
                        await Task.Yield();
                        return Constant;
                    })());
                }
            }
        }
    }
}
