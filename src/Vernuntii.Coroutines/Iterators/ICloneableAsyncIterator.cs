namespace Vernuntii.Coroutines.Iterators;

public interface ICloneableAsyncIterator : IAsyncIterator
{
    IAsyncIterator Clone();
}
