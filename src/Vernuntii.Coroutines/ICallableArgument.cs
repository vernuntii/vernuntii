namespace Vernuntii.Coroutines;

public interface ICallableArgument
{
    void Callback<TCompletionSource>(in CoroutineContext context, TCompletionSource completionSource) where TCompletionSource : class;
}
