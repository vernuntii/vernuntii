using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;
using Vernuntii.Coroutines.CompilerServices;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

[AsyncMethodBuilder(typeof(CoroutineMethodBuilder<>))]
public partial struct Coroutine<TResult> : IAwaitableCoroutine, IEquatable<Coroutine<TResult>>
{
    internal readonly bool IsChildCoroutine => _builder is not null;

    internal IChildCoroutine? _builder;
    internal ISiblingCoroutine? _argumentReceiverDelegate;
    internal ValueTask<TResult> _task;

    readonly bool IRelativeCoroutine.IsChildCoroutine => IsChildCoroutine;
    readonly bool IRelativeCoroutine.IsSiblingCoroutine => _argumentReceiverDelegate is not null;

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

    public Coroutine(in ValueTask<TResult> task, ISiblingCoroutine argumentReceiverDelegate)
    {
        _task = task;
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public Coroutine(IValueTaskSource<TResult> source, short token, ISiblingCoroutine argumentReceiverDelegate)
    {
        _task = new ValueTask<TResult>(source, token);
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public Coroutine(Task<TResult> task, ISiblingCoroutine argumentReceiverDelegate)
    {
        _task = new ValueTask<TResult>(task);
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    public Coroutine(TResult result, ISiblingCoroutine argumentReceiverDelegate)
    {
        _task = new ValueTask<TResult>(result);
        _argumentReceiverDelegate = argumentReceiverDelegate;
    }

    internal Coroutine(in ValueTask<TResult> task, IChildCoroutine builder)
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

    public readonly CoroutineAwaiter<TResult> GetAwaiter() => new CoroutineAwaiter<TResult>(_task.GetAwaiter(), _builder, _argumentReceiverDelegate);

    public readonly ConfiguredCoroutineAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext) =>
        new ConfiguredCoroutineAwaitable<TResult>(_task.ConfigureAwait(continueOnCapturedContext), _builder, _argumentReceiverDelegate);

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
