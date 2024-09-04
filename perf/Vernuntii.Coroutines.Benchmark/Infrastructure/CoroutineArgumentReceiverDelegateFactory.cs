using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using FastExpressionCompiler.LightExpression;
using static Vernuntii.Coroutines.Benchmark.Infrastructure.CoroutineArgumentReceiverDelegateClosure;

namespace Vernuntii.Coroutines.Benchmark.Infrastructure;

internal class CoroutineArgumentReceiverCachedDelegateFactory {
    private static readonly ConcurrentDictionary<MethodInfo, Func<Tuple<object, object, object, object>, CoroutineArgumentReceiverDelegate>> _cache
       = new ConcurrentDictionary<MethodInfo, Func<Tuple<object, object, object, object>, CoroutineArgumentReceiverDelegate>>();

    public static CoroutineArgumentReceiverDelegate CreateDelegate<T1, T2, T3, T4>(
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        CoroutineArgumentReceiverDelegate<T1, T2, T3, T4> argumentReceiver)
    {
        var closure = System.Tuple.Create<object, object, object, object>(value1, value2, value3, value4);
        var methodInfo = argumentReceiver.Method;
        var factory = _cache.GetOrAdd(methodInfo, CreateFactory<T1, T2, T3, T4>(methodInfo));
        return factory(closure);
    }

    private static Func<Tuple<object, object, object, object>, CoroutineArgumentReceiverDelegate> CreateFactory<T1, T2, T3, T4>(MethodInfo methodInfo)
    {
        // Create the parameters for the lambda
        var closureParam = Expression.Parameter(typeof(Tuple<object, object, object, object>), "closure");
        var argumentReceiverParam = Expression.Parameter(typeof(CoroutineArgumentReceiver).MakeByRefType(), "argumentReceiver");

        // Create the expression to call the original delegate
        var originalDelegateInvoke = Expression.Call(
            Expression.Constant(null, typeof(CoroutineArgumentReceiverDelegate<T1, T2, T3, T4>)),
            methodInfo,
            Expression.Convert(closureParam, typeof(Tuple<T1, T2, T3, T4>)),
            argumentReceiverParam
        );

        // Create the lambda expression for the factory
        var lambda = Expression.Lambda<Func<Tuple<object, object, object, object>, CoroutineArgumentReceiverDelegate>>(
            Expression.Lambda<CoroutineArgumentReceiverDelegate>(originalDelegateInvoke, argumentReceiverParam),
            closureParam
        );

        // Compile the lambda expression into a factory
        return lambda.CompileFast();
    }
}

internal class CoroutineArgumentReceiverDelegateFactory
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Tuple<T1, T2, T3, T4> CreateTuple<T1, T2, T3, T4>(
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4) =>
        new(value1, value2, value3, value4);

    public static CoroutineArgumentReceiverDelegate CreateDelegate<T1, T2, T3, T4>(
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        CoroutineArgumentReceiverDelegate<T1, T2, T3, T4> argumentReceiver)
    {
        var closure = CreateTuple(value1, value2, value3, value4);

        // Create the parameters for the lambda
        var argumentReceiverParam = Expression.Parameter(typeof(CoroutineArgumentReceiver).MakeByRefType(), "argumentReceiver");

        // Create the expression to call the original delegate
        var originalDelegateInvoke = Expression.Invoke(
            Expression.Constant(argumentReceiver),
            Expression.Constant(closure),
            argumentReceiverParam
        );

        // Create the lambda expression
        var lambda = Expression.Lambda<CoroutineArgumentReceiverDelegate>(originalDelegateInvoke, argumentReceiverParam);

        // Compile the lambda expression into a delegate
        return lambda.CompileFast();
    }
}
