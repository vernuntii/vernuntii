using Vernuntii.Coroutines;

namespace Vernuntii.Infrastructure;

public class ReleativeCoroutineHolder
{
    internal IRelativeCoroutine Coroutine { get; init; }

    internal ReleativeCoroutineHolder(IRelativeCoroutine coroutine) => Coroutine = coroutine;
}
