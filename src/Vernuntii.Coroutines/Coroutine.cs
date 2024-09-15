using System.Runtime.InteropServices;
using System.Threading.Tasks.Sources;
using Vernuntii.Coroutines.CompilerServices;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

/**
 * Never attempt to use Unsafe.As to access _task.
 */
[AsyncMethodBuilder(typeof(CoroutineMethodBuilder))]
[StructLayout(LayoutKind.Auto)]
public partial struct Coroutine : IRelativeCoroutine, IEquatable<Coroutine>
{
    internal readonly object? _coroutineActioner;
    internal CoroutineAction _coroutineAction;
    internal ValueTask _task;

    readonly object? IRelativeCoroutine.CoroutineActioner => _coroutineActioner;
    readonly CoroutineAction IRelativeCoroutine.CoroutineAction => _coroutineAction;

    public Coroutine(in ValueTask task)
    {
        _task = task;
    }

    public Coroutine(IValueTaskSource source, short token)
    {
        _task = new ValueTask(source, token);
    }

    public Coroutine(Task task)
    {
        _task = new ValueTask(task);
    }

    public Coroutine(in ValueTask task, ISiblingCoroutine siblingCoroutine)
    {
        _task = task;
        _coroutineActioner = siblingCoroutine;
        _coroutineAction = CoroutineAction.Sibling;
    }

    public Coroutine(IValueTaskSource source, short token, ISiblingCoroutine siblingCoroutine)
    {
        _task = new ValueTask(source, token);
        _coroutineActioner = siblingCoroutine;
        _coroutineAction = CoroutineAction.Sibling;
    }

    public Coroutine(Task task, ISiblingCoroutine siblingCoroutine)
    {
        _task = new ValueTask(task);
        _coroutineActioner = siblingCoroutine;
        _coroutineAction = CoroutineAction.Sibling;
    }

    internal Coroutine(in ValueTask task, IChildCoroutine childCoroutine)
    {
        _task = task;
        _coroutineActioner = childCoroutine;
        _coroutineAction = CoroutineAction.Child;
    }

    void IRelativeCoroutine.MarkCoroutineAsActedOn() => _coroutineAction = CoroutineAction.None;

    public readonly CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(_task.GetAwaiter(), _coroutineActioner, _coroutineAction);

    public readonly ConfiguredCoroutineAwaitable ConfigureAwait(bool continueOnCapturedContext) =>
        new(_task.ConfigureAwait(continueOnCapturedContext), _coroutineActioner, _coroutineAction);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Coroutine(Task task) => new(task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Coroutine(ValueTask task) => new(task);

    public readonly IAsyncIterator GetAsyncIterator(in CoroutineContext additiveContext = default) => new AsyncIteratorImpl<Nothing>(this, in additiveContext);

    public readonly bool Equals(Coroutine other) => CoroutineEqualityComparer.Equals(in this, in other);

    /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="object"/>.</summary>
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Coroutine co && Equals(co);

    /// <summary>Returns a value indicating whether two <see cref="ValueTask"/> values are equal.</summary>
    public static bool operator ==(Coroutine left, Coroutine right) => left.Equals(right);

    /// <summary>Returns a value indicating whether two <see cref="ValueTask"/> values are not equal.</summary>
    public static bool operator !=(Coroutine left, Coroutine right) => !left.Equals(right);

    public override readonly int GetHashCode() => CoroutineEqualityComparer.GetHashCode(this);
}
