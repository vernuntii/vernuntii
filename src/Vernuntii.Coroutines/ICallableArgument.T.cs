using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public interface ICallableArgument<TCompletionSource> : ICallableArgument
    where TCompletionSource : class
{
    void Callback(in CoroutineContext context, TCompletionSource completionSource);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICallableArgument.Callback<T>(in CoroutineContext context, T completionSource) =>
        Callback(in context, Unsafe.As<TCompletionSource>(completionSource)!);
}
