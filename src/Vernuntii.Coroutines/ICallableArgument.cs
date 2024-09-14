namespace Vernuntii.Coroutines;

public interface ICallableArgument
{
    void Callback(in CoroutineContext context);
}
