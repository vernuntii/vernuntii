using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Vernuntii.Coroutines.Benchmark
{
    public class Benchmarks
    {
        //[Benchmark]
        //public async Task Scenario1()
        //{
        //    await Task.Delay(1).ConfigureAwait(false);
        //}

        [Benchmark]
        public async Task Scenario2()
        {
            await Task.Run(async () => await Coroutine.Start(async () => await Task.Delay(1).ConfigureAwait(false)).ConfigureAwait(false)).ConfigureAwait(false);
            //await Task.Delay(1).ConfigureAwait(false);
        }
    }
}
