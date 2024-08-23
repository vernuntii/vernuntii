namespace Vernuntii.Coroutines;

internal interface IClosure
{
    T InvokeClosured<T>(Delegate delegateReference);
}
