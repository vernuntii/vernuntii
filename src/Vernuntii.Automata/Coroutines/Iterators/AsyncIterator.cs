using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vernuntii.Coroutines.Iterators;

internal static class AsyncIterator
{
    internal static readonly Key s_asyncIteratorKey = new Key(Encoding.ASCII.GetBytes(nameof(AsyncIterator<VoidResult>)));
}

public enum AsyncIteratorContextServiceOperationState
{
    RequiresSupply = 1,
    ArgumentSupplied = 2,
    AwaiterCompletionNotifierSupplied = 4
}

internal record AsyncIteratorContextServiceOperation
{
    public static readonly AsyncIteratorContextServiceOperation RequiresSupply = new AsyncIteratorContextServiceOperation() { State = AsyncIteratorContextServiceOperationState.RequiresSupply };

    internal ICallableArgument? Argument { get; init; }
    internal IKey? ArgumentKey { get; init; }
    internal ValueTask ExternTaskCompletionNotifier { get; init; }
    internal AsyncIteratorContextServiceOperationState State { get; init; }
}

internal class AsyncIteratorContextService(AsyncIteratorContextServiceOperation initialOperation)
{
    private AsyncIteratorContextServiceOperation _currentOperation = initialOperation;

    public AsyncIteratorContextServiceOperation CurrentOperation => _currentOperation;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowIfNotRequiringSupply(AsyncIteratorContextServiceOperation operation)
    {
        if ((operation.State & AsyncIteratorContextServiceOperationState.RequiresSupply) == 0) {
            throw new InvalidOperationException($"The currnet operation state {operation.State} differs from the expected state {AsyncIteratorContextServiceOperationState.RequiresSupply}");
        }
    }

    internal void SupplyArgument(ICallableArgument argument, IKey argumentKey)
    {
        var operation = _currentOperation;
        ThrowIfNotRequiringSupply(operation);
        _currentOperation = new AsyncIteratorContextServiceOperation() {
            State = AsyncIteratorContextServiceOperationState.ArgumentSupplied | AsyncIteratorContextServiceOperationState.RequiresSupply,
            Argument = argument,
            ArgumentKey = argumentKey
        };
    }

    internal void SupplyAwaiterCompletionNotifier<TAwaiter>(ref TAwaiter awaiter) where TAwaiter : INotifyCompletion
    {
        var operation = _currentOperation;
        ThrowIfNotRequiringSupply(operation);
        var externTaskCompletionNotifierSource = new ValueTaskCompletionSource<VoidResult>();
        _currentOperation = new AsyncIteratorContextServiceOperation() {
            State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied | (operation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied),
            ExternTaskCompletionNotifier = externTaskCompletionNotifierSource.CreateValueTask()
        };
        awaiter.OnCompleted(() => externTaskCompletionNotifierSource.SetDefaultResult());
    }

    internal void SupplyAwaiterCriticalCompletionNotifier<TAwaiter>(ref TAwaiter awaiter) where TAwaiter : ICriticalNotifyCompletion
    {
        var operation = _currentOperation;
        ThrowIfNotRequiringSupply(operation);
        var externTaskCompletionNotifierSource = new ValueTaskCompletionSource<VoidResult>();
        _currentOperation = new AsyncIteratorContextServiceOperation() {
            State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied | (operation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied),
            ExternTaskCompletionNotifier = externTaskCompletionNotifierSource.CreateValueTask(),
            Argument = operation.Argument,
            ArgumentKey = operation.ArgumentKey
        };
        awaiter.UnsafeOnCompleted(() => externTaskCompletionNotifierSource.SetDefaultResult());
    }

    internal void RequiresSupply() =>
        _currentOperation = AsyncIteratorContextServiceOperation.RequiresSupply;
}

internal class AsyncIterator<TResult> : IAsyncIterator, IAsyncIterator<TResult>
{
    readonly ConfiguredAwaitableCoroutine<TResult>.ConfiguredCoroutineAwaiter _coroutineAwaiter;
    CoroutineContext _capturedContext;
    readonly bool _isCoroutineGeneric;

    public AsyncIterator(Coroutine coroutine)
    {
        var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
        _coroutineAwaiter = Unsafe.As<ConfiguredAwaitableCoroutine.ConfiguredCoroutineAwaiter, ConfiguredAwaitableCoroutine<TResult>.ConfiguredCoroutineAwaiter>(ref coroutineAwaiter);
        _isCoroutineGeneric = false;
    }

    public AsyncIterator(Func<Coroutine<TResult>> coroutine)
    {
        _coroutineAwaiter = coroutine().ConfigureAwait(false).GetAwaiter();
        _isCoroutineGeneric = true;
    }

    public object Current { get; } = null!;

    private void BequestContext(ref CoroutineContext context, in CoroutineContext contextToBequest)
    {
        context.InheritContext(in contextToBequest);
        context._bequestContext = null;
        ref var capturedContext = ref _capturedContext;
        capturedContext = context;
        capturedContext._keyedServices = capturedContext.KeyedServices.Remove(AsyncIterator.s_asyncIteratorKey);
    }

    private void BequestCoroutineContext<TCoroutineAwaiter>(in TCoroutineAwaiter coroutineAwaiter, AsyncIteratorContextService contextService) where TCoroutineAwaiter : IRelativeCoroutine
    {
        var scope = new CoroutineScope();
        var context = new CoroutineContext();
        context._keyedServices = ImmutableDictionary.CreateRange<Key, object>([new(AsyncIterator.s_asyncIteratorKey, contextService)]);
        context._keyedServicesToBequest = ImmutableDictionary.CreateRange<Key, object>([new(CoroutineScope.s_coroutineScopeKey, scope)]);
        context._bequesterOrigin = CoroutineContextBequesterOrigin.ContextBequester;
        context._bequestContext = BequestContext;
        CoroutineMethodBuilderCore.PreprocessCoroutine(ref Unsafe.AsRef(in coroutineAwaiter), ref context);
    }

    public async Coroutine<bool> MoveNext()
    {
        if (_coroutineAwaiter.IsCompleted) {
            return false;
        }

        var contextService = new AsyncIteratorContextService(AsyncIteratorContextServiceOperation.RequiresSupply);
        BequestCoroutineContext(in _coroutineAwaiter, contextService);

        var resultStateMachine = _capturedContext._resultStateMachine ?? throw new InvalidOperationException("The targeting coroutine does not have a state machine");
        if (resultStateMachine is not ICoroutineStateMachineBox stateMachine) {
            throw new InvalidOperationException("The targeting coroutine has an invalid state machine");
        }

        var currentOperation = contextService.CurrentOperation;
        while (currentOperation.State == AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied) {
            await currentOperation.ExternTaskCompletionNotifier;
            stateMachine.MoveNext();
            currentOperation = contextService.CurrentOperation;
        };

        if ((currentOperation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied) != 0) {
            if ((currentOperation.State & AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied) == 0) {
                throw new InvalidOperationException();
            }

            var argumentsReceiver = new CoroutineArgumentReceiver(ref _capturedContext);
            Debug.Assert(currentOperation.Argument is not null);
            Debug.Assert(currentOperation.ArgumentKey is not null);
            argumentsReceiver.ReceiveCallableArgument(currentOperation.Argument, currentOperation.ArgumentKey);
            await currentOperation.ExternTaskCompletionNotifier;
            stateMachine.MoveNext();
        }

        return true;
    }

    public Coroutine YieldReturn<TYieldResult>(TYieldResult result) => throw new NotImplementedException();
    public Coroutine Return() => throw new NotImplementedException();
    public Coroutine Return(TResult result) => throw new NotImplementedException();
    public Coroutine Throw(Exception e) => throw new NotImplementedException();
}
