namespace Vernuntii.Coroutines;

internal interface ICallableArgument
{
    void Callback(in CoroutineContext context);
}
