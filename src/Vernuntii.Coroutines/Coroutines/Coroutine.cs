using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;
using Vernuntii.Coroutines.CompilerServices;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

/**
 * Never attempt to use Unsafe.As to access _task.
 */
[AsyncMethodBuilder(typeof(CoroutineMethodBuilder))]
public partial struct Coroutine : IAwaitableCoroutine, IEquatable<Coroutine>
{
    internal readonly bool IsChildCoroutine {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            return _builder is not null;
        }
    }

    internal IChildCoroutine? _builder;
    internal ISiblingCoroutine? _argumentReceiverDelegate;
    internal ValueTask _task;

    readonly bool IRelativeCoroutine.IsChildCoroutine => IsChildCoroutine;
    readonly bool IRelativeCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

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

    public Coroutine(in ValueTask task, ISiblingCoroutine argumentReceiverDelegate)
    {
        _task = task;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public Coroutine(IValueTaskSource source, short token, ISiblingCoroutine argumentReceiverDelegate)
    {
        _task = new ValueTask(source, token);
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public Coroutine(Task task, ISiblingCoroutine argumentReceiverDelegate)
    {
        _task = new ValueTask(task);
    }

    internal Coroutine(in ValueTask task, IChildCoroutine builder)
    {
        _task = task;
        _builder = builder;
    }

    readonly void IChildCoroutine.InheritCoroutineContext(in CoroutineContext context)
    {
        Debug.Assert(_builder != null);
        _builder.InheritCoroutineContext(in context);
    }

    readonly void IChildCoroutine.StartCoroutine()
    {
        Debug.Assert(_builder != null);
        _builder.StartCoroutine();
    }

    readonly void ISiblingCoroutine.AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        Debug.Assert(_argumentReceiverDelegate is not null);
        _argumentReceiverDelegate.AcceptCoroutineArgumentReceiver(ref argumentReceiver);
    }

    void IAwaitableCoroutine.MarkCoroutineAsHandled()
    {
        _builder = null;
        _argumentReceiverDelegate = null;
    }

    public readonly CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(_task.GetAwaiter(), _builder, _argumentReceiverDelegate);

    public readonly ConfiguredCoroutineAwaitable ConfigureAwait(bool continueOnCapturedContext) =>
        new(_task.ConfigureAwait(continueOnCapturedContext), _builder, _argumentReceiverDelegate);

    public readonly IAsyncIterator GetAsyncIterator() => new AsyncIteratorImpl<Nothing>(this);

    public readonly bool Equals(Coroutine other) => CoroutineEqualityComparer.Equals(in this, in other);

    /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="object"/>.</summary>
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Coroutine co && Equals(co);

    /// <summary>Returns a value indicating whether two <see cref="ValueTask"/> values are equal.</summary>
    public static bool operator ==(Coroutine left, Coroutine right) => left.Equals(right);

    /// <summary>Returns a value indicating whether two <see cref="ValueTask"/> values are not equal.</summary>
    public static bool operator !=(Coroutine left, Coroutine right) => !left.Equals(right);

    public override readonly int GetHashCode() => CoroutineEqualityComparer.GetHashCode(this);
}
