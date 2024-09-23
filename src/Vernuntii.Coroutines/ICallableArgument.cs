namespace Vernuntii.Coroutines;

public interface ICallableArgument
{
    internal void Callback(in CoroutineArgumentReceiver argumentReceiver, in Key argumentKey, object completionSource) => throw new NotImplementedException();
}

public interface ICallableArgument<in TCompletionSource> : ICallableArgument where TCompletionSource : class, ICoroutineCompletionSource
{
    void Callback(in CoroutineContext context, TCompletionSource completionSource);

    void ICallableArgument.Callback(in CoroutineArgumentReceiver argumentReceiver, in Key argumentKey, object completionSource) =>
        argumentReceiver.ReceiveCallableArgument(in argumentKey, this, (TCompletionSource)completionSource);
}
