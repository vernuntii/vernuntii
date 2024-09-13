using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Sources;
using Vernuntii.Coroutines.CompilerServices;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

[AsyncMethodBuilder(typeof(CoroutineMethodBuilder<>))]
[StructLayout(LayoutKind.Auto)]
public partial struct Coroutine<TResult> : IRelativeCoroutine, IEquatable<Coroutine<TResult>>
{
    internal readonly object? _coroutineActioner;
    internal CoroutineAction _coroutineAction;
    internal ValueTask<TResult> _task;

    readonly object? IRelativeCoroutine.CoroutineActioner => _coroutineActioner;
    readonly CoroutineAction IRelativeCoroutine.CoroutineAction => _coroutineAction;

    public Coroutine(in ValueTask<TResult> task)
    {
        _task = task;
    }

    public Coroutine(IValueTaskSource<TResult> source, short token)
    {
        _task = new ValueTask<TResult>(source, token);
    }

    public Coroutine(Task<TResult> task)
    {
        _task = new ValueTask<TResult>(task);
    }

    public Coroutine(TResult result)
    {
        _task = new ValueTask<TResult>(result);
    }

    public Coroutine(in ValueTask<TResult> task, ISiblingCoroutine siblingCoroutine)
    {
        _task = task;
        _coroutineActioner = siblingCoroutine;
        _coroutineAction = CoroutineAction.Sibling;
    }

    public Coroutine(IValueTaskSource<TResult> source, short token, ISiblingCoroutine siblingCoroutine)
    {
        _task = new ValueTask<TResult>(source, token);
        _coroutineActioner = siblingCoroutine;
        _coroutineAction = CoroutineAction.Sibling;
    }

    public Coroutine(Task<TResult> task, ISiblingCoroutine siblingCoroutine)
    {
        _task = new ValueTask<TResult>(task);
        _coroutineActioner = siblingCoroutine;
        _coroutineAction = CoroutineAction.Sibling;
    }

    public Coroutine(TResult result, ISiblingCoroutine siblingCoroutine)
    {
        _task = new ValueTask<TResult>(result);
        _coroutineAction = CoroutineAction.Sibling;
        _coroutineActioner = siblingCoroutine;
    }

    internal Coroutine(in ValueTask<TResult> task, IChildCoroutine childCoroutine)
    {
        _task = task;
        _coroutineActioner = childCoroutine;
        _coroutineAction = CoroutineAction.Child;
    }

    void IRelativeCoroutine.MarkCoroutineAsActedOn() => _coroutineAction = CoroutineAction.None;

    public readonly CoroutineAwaiter<TResult> GetAwaiter() => new CoroutineAwaiter<TResult>(_task.GetAwaiter(), _coroutineActioner, _coroutineAction);

    public readonly ConfiguredCoroutineAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext) =>
        new ConfiguredCoroutineAwaitable<TResult>(_task.ConfigureAwait(continueOnCapturedContext), _coroutineActioner, _coroutineAction);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Coroutine<TResult>(Task<TResult> task) => new(task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Coroutine<TResult>(ValueTask<TResult> task) => new(task);

    public readonly IAsyncIterator<TResult> GetAsyncIterator() => new AsyncIteratorImpl<TResult>(this);

    public readonly bool Equals(Coroutine<TResult> other) => CoroutineEqualityComparer.Equals(in this, in other);

    /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="object"/>.</summary>
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Coroutine<TResult> co && Equals(co);

    /// <summary>Returns a value indicating whether two <see cref="ValueTask"/> values are equal.</summary>
    public static bool operator ==(Coroutine<TResult> left, Coroutine<TResult> right) => left.Equals(right);

    /// <summary>Returns a value indicating whether two <see cref="ValueTask"/> values are not equal.</summary>
    public static bool operator !=(Coroutine<TResult> left, Coroutine<TResult> right) => !left.Equals(right);

    public override readonly int GetHashCode() => CoroutineEqualityComparer.GetHashCode(this);
}
