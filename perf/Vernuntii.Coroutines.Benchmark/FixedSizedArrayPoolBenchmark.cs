using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Vernuntii.Collections;

namespace Vernuntii.Coroutines.Benchmark;

[MemoryDiagnoser]
[MediumRunJob]
public class FixedSizedArrayPoolBenchmark
{
    private const int RUNS = 1_000_000;
    private const int RUN_4 = 4;
    private const int RUN_64 = 64;
    private const int RUN_256 = 256;

    private Dictionary<int, FixedSizedArrayPool<FixedSizedValue>> FixedsizedArrayPools { get; set; }

    [IterationSetup]
    public void Setup()
    {
        FixedsizedArrayPools = new Dictionary<int, FixedSizedArrayPool<FixedSizedValue>> {
            { RUN_4, new FixedSizedArrayPool<FixedSizedValue>(RUN_4) },
            { RUN_64, new FixedSizedArrayPool<FixedSizedValue>(RUN_64) },
            { RUN_256, new FixedSizedArrayPool<FixedSizedValue>(RUN_256) }
        };
    }

    [Benchmark]
    [Arguments(RUN_4)]
    [Arguments(RUN_64)]
    [Arguments(RUN_256)]
    [MethodImpl(MethodImplOptions.NoOptimization)]
    public void NewArrayWithGC(int arraySize)
    {
        for (var run = RUNS; run > 0; run--) {
            var array = new FixedSizedValue[arraySize];
            _ = array.Length;
        }
    }

    [Benchmark]
    [Arguments(RUN_4)]
    [Arguments(RUN_64)]
    [Arguments(RUN_256)]
    [MethodImpl(MethodImplOptions.NoOptimization)]
    public void RentArrayWithReturn(int arraySize)
    {
        var arrayPool = FixedsizedArrayPools[arraySize];

        for (var run = RUNS; run > 0; run--) {
            var array = arrayPool.Rent();
            _ = array.Length;
            arrayPool.Return(array);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    internal struct FixedSizedValue
    {
        [FieldOffset(0)]
        object _field;
    }
}
