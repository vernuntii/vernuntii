using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public sealed record Closure<T1>(T1 Value1) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1,TResult>>(delegateReference).Invoke(Value1);
    }
}

public sealed record Closure<T1, T2>(T1 Value1, T2 Value2) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2,TResult>>(delegateReference).Invoke(Value1, Value2);
    }
}

public sealed record Closure<T1, T2, T3>(T1 Value1, T2 Value2, T3 Value3) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3,TResult>>(delegateReference).Invoke(Value1, Value2, Value3);
    }
}

public sealed record Closure<T1, T2, T3, T4>(T1 Value1, T2 Value2, T3 Value3, T4 Value4) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3, T4,TResult>>(delegateReference).Invoke(Value1, Value2, Value3, Value4);
    }
}

public sealed record Closure<T1, T2, T3, T4, T5>(T1 Value1, T2 Value2, T3 Value3, T4 Value4, T5 Value5) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3, T4, T5,TResult>>(delegateReference).Invoke(Value1, Value2, Value3, Value4, Value5);
    }
}

public sealed record Closure<T1, T2, T3, T4, T5, T6>(T1 Value1, T2 Value2, T3 Value3, T4 Value4, T5 Value5, T6 Value6) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3, T4, T5, T6,TResult>>(delegateReference).Invoke(Value1, Value2, Value3, Value4, Value5, Value6);
    }
}

public sealed record Closure<T1, T2, T3, T4, T5, T6, T7>(T1 Value1, T2 Value2, T3 Value3, T4 Value4, T5 Value5, T6 Value6, T7 Value7) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7,TResult>>(delegateReference).Invoke(Value1, Value2, Value3, Value4, Value5, Value6, Value7);
    }
}

public sealed record Closure<T1, T2, T3, T4, T5, T6, T7, T8>(T1 Value1, T2 Value2, T3 Value3, T4 Value4, T5 Value5, T6 Value6, T7 Value7, T8 Value8) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8,TResult>>(delegateReference).Invoke(Value1, Value2, Value3, Value4, Value5, Value6, Value7, Value8);
    }
}

public sealed record Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 Value1, T2 Value2, T3 Value3, T4 Value4, T5 Value5, T6 Value6, T7 Value7, T8 Value8, T9 Value9) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9,TResult>>(delegateReference).Invoke(Value1, Value2, Value3, Value4, Value5, Value6, Value7, Value8, Value9);
    }
}

public sealed record Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 Value1, T2 Value2, T3 Value3, T4 Value4, T5 Value5, T6 Value6, T7 Value7, T8 Value8, T9 Value9, T10 Value10) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10,TResult>>(delegateReference).Invoke(Value1, Value2, Value3, Value4, Value5, Value6, Value7, Value8, Value9, Value10);
    }
}

public sealed record Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 Value1, T2 Value2, T3 Value3, T4 Value4, T5 Value5, T6 Value6, T7 Value7, T8 Value8, T9 Value9, T10 Value10, T11 Value11) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11,TResult>>(delegateReference).Invoke(Value1, Value2, Value3, Value4, Value5, Value6, Value7, Value8, Value9, Value10, Value11);
    }
}

public sealed record Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 Value1, T2 Value2, T3 Value3, T4 Value4, T5 Value5, T6 Value6, T7 Value7, T8 Value8, T9 Value9, T10 Value10, T11 Value11, T12 Value12) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12,TResult>>(delegateReference).Invoke(Value1, Value2, Value3, Value4, Value5, Value6, Value7, Value8, Value9, Value10, Value11, Value12);
    }
}

public sealed record Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 Value1, T2 Value2, T3 Value3, T4 Value4, T5 Value5, T6 Value6, T7 Value7, T8 Value8, T9 Value9, T10 Value10, T11 Value11, T12 Value12, T13 Value13) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13,TResult>>(delegateReference).Invoke(Value1, Value2, Value3, Value4, Value5, Value6, Value7, Value8, Value9, Value10, Value11, Value12, Value13);
    }
}

public sealed record Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 Value1, T2 Value2, T3 Value3, T4 Value4, T5 Value5, T6 Value6, T7 Value7, T8 Value8, T9 Value9, T10 Value10, T11 Value11, T12 Value12, T13 Value13, T14 Value14) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14,TResult>>(delegateReference).Invoke(Value1, Value2, Value3, Value4, Value5, Value6, Value7, Value8, Value9, Value10, Value11, Value12, Value13, Value14);
    }
}

public sealed record Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 Value1, T2 Value2, T3 Value3, T4 Value4, T5 Value5, T6 Value6, T7 Value7, T8 Value8, T9 Value9, T10 Value10, T11 Value11, T12 Value12, T13 Value13, T14 Value14, T15 Value15) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15,TResult>>(delegateReference).Invoke(Value1, Value2, Value3, Value4, Value5, Value6, Value7, Value8, Value9, Value10, Value11, Value12, Value13, Value14, Value15);
    }
}

public sealed record Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 Value1, T2 Value2, T3 Value3, T4 Value4, T5 Value5, T6 Value6, T7 Value7, T8 Value8, T9 Value9, T10 Value10, T11 Value11, T12 Value12, T13 Value13, T14 Value14, T15 Value15, T16 Value16) : IClosure
{
    TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
    {
        return Unsafe.As<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16,TResult>>(delegateReference).Invoke(Value1, Value2, Value3, Value4, Value5, Value6, Value7, Value8, Value9, Value10, Value11, Value12, Value13, Value14, Value15, Value16);
    }
}
