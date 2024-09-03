using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Vernuntii.Coroutines.Benchmark
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class CallInternalBenchmark
    {
        public class ObjectPool<T> where T : class, new()
        {
            private readonly ConcurrentBag<T> _objects;

            public ObjectPool()
            {
                _objects = new ConcurrentBag<T>();
            }

            public T Get() => _objects.TryTake(out var item) ? item : new T();

            public void Return(T item) => _objects.Add(item);
        }

        private class ArgumentReceiverClosure
        {
            internal static readonly ObjectPool<ArgumentReceiverClosure> s_pool = new();

            internal Delegate _provider = null!;
            internal IClosure? _providerClosure = null!;
            internal object _completionSource = null!;
            internal ArgumentReceiverDelegate _argumentReceiverHandler = null!;
        }

        private delegate void ArgumentReceiverDelegate(ArgumentReceiverClosure closure, ref CoroutineArgumentReceiver argumentReceiver);

        private static void ArgumentReceiver(ArgumentReceiverClosure closure, ref CoroutineArgumentReceiver argumentReceiver) =>
            closure._argumentReceiverHandler(closure, ref argumentReceiver);

        private static readonly MethodInfo s_argumentReceiverDelegateMethod =
            ((ArgumentReceiverDelegate)ArgumentReceiver).Method;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Coroutine<TResult> CallInternalWithPoolClosure<TResult>(Delegate provider, IClosure? providerClosure)
        {
            var completionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
            var argumentReceiverClosure = ArgumentReceiverClosure.s_pool.Get();
            argumentReceiverClosure._provider = provider;
            argumentReceiverClosure._providerClosure = providerClosure;
            argumentReceiverClosure._completionSource = completionSource;
            argumentReceiverClosure._argumentReceiverHandler = HandleArgumentReceiver;
            var argumentReceiverDelegate = s_argumentReceiverDelegateMethod.CreateDelegate<CoroutineArgumentReceiverDelegate>(argumentReceiverClosure);
            return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), argumentReceiverDelegate);
            static void HandleArgumentReceiver(ArgumentReceiverClosure closure, ref CoroutineArgumentReceiver argumentReceiver)
            {
                var completionSource = Unsafe.As<ValueTaskCompletionSource<TResult>>(closure._completionSource);
                var argument = new Arguments.CallArgument<TResult>(
                    closure._provider,
                    closure._providerClosure,
                    completionSource);
                ArgumentReceiverClosure.s_pool.Return(closure);
                argumentReceiver.ReceiveCallableArgument(in Arguments.s_callArgumentType, in argument, completionSource);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Coroutine<TResult> CallInternalWithImplicitClosure<TResult>(Delegate provider, IClosure? providerClosure)
        {
            var completionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
            return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

            void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
            {
                var argument = new Arguments.CallArgument<TResult>(provider, providerClosure, completionSource);
                argumentReceiver.ReceiveCallableArgument(in Arguments.s_callArgumentType, in argument, completionSource);
            }
        }

        async static Coroutine<int> CoroutineReturningNumber(int number)
        {
            return number;
        }

        private static readonly Func<int, Coroutine<int>> CoroutineReturningNumberDelegate = CoroutineReturningNumber;

        [Benchmark]
        public async Task CallInternal_ImplicitClosure()
        {
            await Coroutine.Start(() => CallInternalWithImplicitClosure<int>(CoroutineReturningNumberDelegate, new Closure<int>(2))).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task CallInternal_PooledClosure()
        {
            await Coroutine.Start(() => CallInternalWithPoolClosure<int>(CoroutineReturningNumberDelegate, new Closure<int>(2))).ConfigureAwait(false);
        }
    }
}
