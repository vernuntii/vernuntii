namespace Vernuntii.Coroutines;

public interface IClosure
{
    int Length { get; }

    T InvokeDelegateWithClosure<T>(Delegate delegateReference);
}
