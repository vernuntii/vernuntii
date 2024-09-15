namespace Vernuntii.Coroutines.Iterators;

public interface ICloneableAsyncIterator<TResult> : IAsyncIterator<TResult>
{
    IAsyncIterator Clone();
}
