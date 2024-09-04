using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Vernuntii.Coroutines.Benchmark.Infrastructure;

namespace Vernuntii.Coroutines.Benchmark
{
    public static class CallInternalExtensions
    {
        //static void ArgumentReceiverDelegate(Tuple<Func<TProviderClosure, Coroutine<TResult>>, TProviderClosure, ValueTaskCompletionSource<TResult>> closure, ref CoroutineArgumentReceiver argumentReceiver)
        //{
        //    var argument = new CallArgument<TResult>(closure.provider, providerClosure, completionSource);
        //    argumentReceiver.ReceiveCallableArgument(in s_callArgumentType, in argument, completionSource);
        //}
    }

    [MemoryDiagnoser]
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
            internal ITuple? _providerClosure = null!;
            internal object _completionSource = null!;
            internal ArgumentReceiverDelegate _argumentReceiverHandler = null!;
        }

        private delegate void ArgumentReceiverDelegate(ArgumentReceiverClosure closure, ref CoroutineArgumentReceiver argumentReceiver);

        private static void ArgumentReceiver(ArgumentReceiverClosure closure, ref CoroutineArgumentReceiver argumentReceiver) =>
            closure._argumentReceiverHandler(closure, ref argumentReceiver);

        private static readonly MethodInfo s_argumentReceiverDelegateMethod =
            ((ArgumentReceiverDelegate)ArgumentReceiver).Method;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Coroutine<TResult> CallInternalWithImplicitClosure<TResult>(Delegate provider, int providerClosure)
        {
            var completionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
            return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

            void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
            {
                var argument = new CallArgument<int, TResult>(provider, providerClosure, isProviderWithClosure: true, completionSource);
                argumentReceiver.ReceiveCallableArgument(in s_callArgumentType, in argument, completionSource);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Coroutine<TResult> CallInternalWithPoolClosure<TResult>(Delegate provider, ITuple? providerClosure)
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
                var argument = new CallArgument<ITuple?, TResult>(
                    closure._provider,
                    closure._providerClosure,
                    isProviderWithClosure: true,
                    completionSource);
                ArgumentReceiverClosure.s_pool.Return(closure);
                argumentReceiver.ReceiveCallableArgument(in s_callArgumentType, in argument, completionSource);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Coroutine<TResult> CallInternalWithClosedDelegate<TProviderClosure, TResult>(Delegate provider, TProviderClosure providerClosure, bool isProviderWithClosure)
        {
            var completionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
            CoroutineArgumentReceiverDelegateWithClosure<Delegate, TProviderClosure, bool, ValueTaskCompletionSource<TResult>> argumentReceiver = AcceptArgumentReceiver;
            var argumentReceiverClosure = CoroutineArgumentReceiverDelegateClosure.Create(provider, providerClosure, isProviderWithClosure, completionSource, argumentReceiver);
            return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), argumentReceiverClosure.CoroutineArgumentReceiver);

            static void AcceptArgumentReceiver(
                Tuple<Delegate, TProviderClosure, bool, ValueTaskCompletionSource<TResult>, CoroutineArgumentReceiverDelegateWithClosure<Delegate, TProviderClosure, bool, ValueTaskCompletionSource<TResult>>> closure,
                ref CoroutineArgumentReceiver argumentReceiver)
            {
                var argument = new CallArgument<TProviderClosure, TResult>(closure.Item1, closure.Item2, closure.Item3, closure.Item4);
                argumentReceiver.ReceiveCallableArgument(in s_callArgumentType, in argument, closure.Item4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Coroutine<TResult> CallInternalWithCompiledDelegate<TProviderClosure, TResult>(Func<TProviderClosure, Coroutine<TResult>> provider, TProviderClosure providerClosure, bool isProviderWithClosure)
        {
            var completionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
            CoroutineArgumentReceiverDelegate<Delegate, TProviderClosure, bool, ValueTaskCompletionSource<TResult>> argumentReceiver = AcceptArgumentReceiver;
            var argumentReceiverClosure = CoroutineArgumentReceiverDelegateFactory.CreateDelegate(provider, providerClosure, isProviderWithClosure, completionSource, argumentReceiver);
            return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), argumentReceiverClosure);

            static void AcceptArgumentReceiver(
                Tuple<Delegate, TProviderClosure, bool, ValueTaskCompletionSource<TResult>> closure,
                ref CoroutineArgumentReceiver argumentReceiver)
            {
                var argument = new CallArgument<TProviderClosure, TResult>(closure.Item1, closure.Item2, closure.Item3, closure.Item4);
                argumentReceiver.ReceiveCallableArgument(in s_callArgumentType, in argument, closure.Item4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Coroutine<TResult> CallInternalWithCachedCompiledDelegate<TProviderClosure, TResult>(Func<TProviderClosure, Coroutine<TResult>> provider, TProviderClosure providerClosure, bool isProviderWithClosure)
        {
            var completionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
            CoroutineArgumentReceiverDelegate<Delegate, TProviderClosure, bool, ValueTaskCompletionSource<TResult>> argumentReceiver = AcceptArgumentReceiver;
            var argumentReceiverClosure = CoroutineArgumentReceiverCachedDelegateFactory.CreateDelegate(provider, providerClosure, isProviderWithClosure, completionSource, argumentReceiver);
            return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), argumentReceiverClosure);

            static void AcceptArgumentReceiver(
                Tuple<Delegate, TProviderClosure, bool, ValueTaskCompletionSource<TResult>> closure,
                ref CoroutineArgumentReceiver argumentReceiver)
            {
                var argument = new CallArgument<TProviderClosure, TResult>(closure.Item1, closure.Item2, closure.Item3, closure.Item4);
                argumentReceiver.ReceiveCallableArgument(in s_callArgumentType, in argument, closure.Item4);
            }
        }

        async static Coroutine<int> CoroutineReturningNumber(int number)
        {
            return number;
        }

        [Benchmark(Baseline = true)]
        public async Task CallInternal()
        {
            await Coroutine.Start(() => CallInternal<int, int>(CoroutineReturningNumber, 2, isProviderWithClosure: true)).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task CallInternal_ImplicitClosure()
        {
            await Coroutine.Start(() => CallInternalWithImplicitClosure<int>(CoroutineReturningNumber, 2)).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task CallInternal_PooledClosure()
        {
            await Coroutine.Start(() => CallInternalWithPoolClosure<int>(CoroutineReturningNumber, new Tuple<int>(2))).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task CallInternal_ClosedDelegate()
        {
            await Coroutine.Start(() => CallInternalWithClosedDelegate<int, int>(CoroutineReturningNumber, 2, true)).ConfigureAwait(false);
        }

        [Benchmark]
        public async Task CallInternal_FastExpression()
        {
            await Coroutine.Start(() => CallInternalWithCompiledDelegate(CoroutineReturningNumber, 2, true)).ConfigureAwait(false);
        }
    }
}
