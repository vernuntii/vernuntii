namespace Vernuntii.Coroutines;

internal interface IClosure
{
    T InvokeDelegateWithClosure<T>(Delegate delegateReference);
}
