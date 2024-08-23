namespace Vernuntii.Coroutines;

partial class Effect {
    public static Coroutine Call<T1>(Func<T1, Coroutine> provider, T1 value1) => CallInternal(provider, new Closure<T1>(value1));

    public static Coroutine<TResult> Call<T1,TResult>(Func<T1,Coroutine<TResult>> provider, T1 value1) => CallInternal<TResult>(provider, new Closure<T1>(value1));

    public static Coroutine Call<T1, T2>(Func<T1, T2, Coroutine> provider, T1 value1, T2 value2) => CallInternal(provider, new Closure<T1, T2>(value1, value2));

    public static Coroutine<TResult> Call<T1, T2,TResult>(Func<T1, T2,Coroutine<TResult>> provider, T1 value1, T2 value2) => CallInternal<TResult>(provider, new Closure<T1, T2>(value1, value2));

    public static Coroutine Call<T1, T2, T3>(Func<T1, T2, T3, Coroutine> provider, T1 value1, T2 value2, T3 value3) => CallInternal(provider, new Closure<T1, T2, T3>(value1, value2, value3));

    public static Coroutine<TResult> Call<T1, T2, T3,TResult>(Func<T1, T2, T3,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3) => CallInternal<TResult>(provider, new Closure<T1, T2, T3>(value1, value2, value3));

    public static Coroutine Call<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Coroutine> provider, T1 value1, T2 value2, T3 value3, T4 value4) => CallInternal(provider, new Closure<T1, T2, T3, T4>(value1, value2, value3, value4));

    public static Coroutine<TResult> Call<T1, T2, T3, T4,TResult>(Func<T1, T2, T3, T4,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3, T4 value4) => CallInternal<TResult>(provider, new Closure<T1, T2, T3, T4>(value1, value2, value3, value4));

    public static Coroutine Call<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Coroutine> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5) => CallInternal(provider, new Closure<T1, T2, T3, T4, T5>(value1, value2, value3, value4, value5));

    public static Coroutine<TResult> Call<T1, T2, T3, T4, T5,TResult>(Func<T1, T2, T3, T4, T5,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5) => CallInternal<TResult>(provider, new Closure<T1, T2, T3, T4, T5>(value1, value2, value3, value4, value5));

    public static Coroutine Call<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, Coroutine> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6) => CallInternal(provider, new Closure<T1, T2, T3, T4, T5, T6>(value1, value2, value3, value4, value5, value6));

    public static Coroutine<TResult> Call<T1, T2, T3, T4, T5, T6,TResult>(Func<T1, T2, T3, T4, T5, T6,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6) => CallInternal<TResult>(provider, new Closure<T1, T2, T3, T4, T5, T6>(value1, value2, value3, value4, value5, value6));

    public static Coroutine Call<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, Coroutine> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7) => CallInternal(provider, new Closure<T1, T2, T3, T4, T5, T6, T7>(value1, value2, value3, value4, value5, value6, value7));

    public static Coroutine<TResult> Call<T1, T2, T3, T4, T5, T6, T7,TResult>(Func<T1, T2, T3, T4, T5, T6, T7,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7) => CallInternal<TResult>(provider, new Closure<T1, T2, T3, T4, T5, T6, T7>(value1, value2, value3, value4, value5, value6, value7));

    public static Coroutine Call<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, Coroutine> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8) => CallInternal(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8>(value1, value2, value3, value4, value5, value6, value7, value8));

    public static Coroutine<TResult> Call<T1, T2, T3, T4, T5, T6, T7, T8,TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8) => CallInternal<TResult>(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8>(value1, value2, value3, value4, value5, value6, value7, value8));

    public static Coroutine Call<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Coroutine> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9) => CallInternal(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9>(value1, value2, value3, value4, value5, value6, value7, value8, value9));

    public static Coroutine<TResult> Call<T1, T2, T3, T4, T5, T6, T7, T8, T9,TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9) => CallInternal<TResult>(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9>(value1, value2, value3, value4, value5, value6, value7, value8, value9));

    public static Coroutine Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Coroutine> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10) => CallInternal(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10));

    public static Coroutine<TResult> Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10,TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10) => CallInternal<TResult>(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10));

    public static Coroutine Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Coroutine> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11) => CallInternal(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11));

    public static Coroutine<TResult> Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11,TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11) => CallInternal<TResult>(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11));

    public static Coroutine Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, Coroutine> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12) => CallInternal(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12));

    public static Coroutine<TResult> Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12,TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12) => CallInternal<TResult>(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12));

    public static Coroutine Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, Coroutine> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13) => CallInternal(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13));

    public static Coroutine<TResult> Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13,TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13) => CallInternal<TResult>(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13));

    public static Coroutine Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, Coroutine> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, T14 value14) => CallInternal(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, value14));

    public static Coroutine<TResult> Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14,TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, T14 value14) => CallInternal<TResult>(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, value14));

    public static Coroutine Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, Coroutine> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, T14 value14, T15 value15) => CallInternal(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, value14, value15));

    public static Coroutine<TResult> Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15,TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, T14 value14, T15 value15) => CallInternal<TResult>(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, value14, value15));

    public static Coroutine Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, Coroutine> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, T14 value14, T15 value15, T16 value16) => CallInternal(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, value14, value15, value16));

    public static Coroutine<TResult> Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16,TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16,Coroutine<TResult>> provider, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, T14 value14, T15 value15, T16 value16) => CallInternal<TResult>(provider, new Closure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(value1, value2, value3, value4, value5, value6, value7, value8, value9, value10, value11, value12, value13, value14, value15, value16));
}
