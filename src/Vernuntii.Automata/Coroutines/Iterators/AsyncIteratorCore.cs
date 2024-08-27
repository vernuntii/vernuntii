using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vernuntii.Coroutines.Iterators;

internal static class AsyncIterator
{
    internal static readonly Key s_asyncIteratorKey = new Key(Encoding.ASCII.GetBytes(nameof(AsyncIterator)));
}

[Flags]
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
    internal IAsyncIterationCompletionSource? ArgumentCompletionSource { get; init; }
    internal ValueTask AwaiterCompletionNotifier { get; init; }
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

    internal void SupplyArgument(IKey argumentKey, ICallableArgument argument, IAsyncIterationCompletionSource argumentCompletionSource)
    {
        var operation = _currentOperation;
        ThrowIfNotRequiringSupply(operation);
        _currentOperation = new AsyncIteratorContextServiceOperation() {
            State = AsyncIteratorContextServiceOperationState.ArgumentSupplied | AsyncIteratorContextServiceOperationState.RequiresSupply,
            ArgumentKey = argumentKey,
            Argument = argument,
            ArgumentCompletionSource = argumentCompletionSource
        };
    }

    internal void SupplyAwaiterCompletionNotifier<TAwaiter>(ref TAwaiter awaiter) where TAwaiter : INotifyCompletion
    {
        var operation = _currentOperation;
        ThrowIfNotRequiringSupply(operation);
        var externTaskCompletionNotifierSource = ValueTaskCompletionSource<VoidResult>.RentFromCache();
        _currentOperation = new AsyncIteratorContextServiceOperation() {
            State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied | (operation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied),
            AwaiterCompletionNotifier = externTaskCompletionNotifierSource.CreateValueTask(),
            ArgumentKey = operation.ArgumentKey,
            Argument = operation.Argument,
            ArgumentCompletionSource = operation.ArgumentCompletionSource
        };
        awaiter.OnCompleted(() => externTaskCompletionNotifierSource.SetDefaultResult());
    }

    internal void SupplyAwaiterCriticalCompletionNotifier<TAwaiter>(ref TAwaiter awaiter) where TAwaiter : ICriticalNotifyCompletion
    {
        var operation = _currentOperation;
        ThrowIfNotRequiringSupply(operation);
        var externTaskCompletionNotifierSource = ValueTaskCompletionSource<VoidResult>.RentFromCache();
        _currentOperation = new AsyncIteratorContextServiceOperation() {
            State = AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied | (operation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied),
            AwaiterCompletionNotifier = externTaskCompletionNotifierSource.CreateValueTask(),
            ArgumentKey = operation.ArgumentKey,
            Argument = operation.Argument,
            ArgumentCompletionSource = operation.ArgumentCompletionSource
        };
        awaiter.UnsafeOnCompleted(() => externTaskCompletionNotifierSource.SetDefaultResult());
    }

    internal void RequiresSupply() =>
        _currentOperation = AsyncIteratorContextServiceOperation.RequiresSupply;
}

internal class AsyncIteratorCore<TReturnResult> : IAsyncIterator, IAsyncIterator<TReturnResult>
{
    private readonly Coroutine<TReturnResult> _coroutine;
    private readonly bool _isCoroutineGeneric;
    private AsyncIteratorContext? _iteratorContext;
    private AsyncIteratorContextServiceOperation? _nextOperation;

    public AsyncIteratorCore(Func<Coroutine> coroutine)
    {
        //_coroutine = Unsafe.As<Coroutine, Coroutine<TResult>>(ref coroutine);
        _isCoroutineGeneric = false;
    }

    public AsyncIteratorCore(Func<Coroutine<TReturnResult>> coroutine)
    {
        _coroutine = coroutine();
        _isCoroutineGeneric = true;
    }

    private void OnBequestCoroutineContext(ref CoroutineContext context, in CoroutineContext contextToBequest)
    {
        var iteratorContext = GetIteratorContext(out _);
        context.InheritContext(in contextToBequest);
        context._bequestContext = null;
        iteratorContext._iteratorAgnosticCoroutineContext = context; // Copy befor making context async-iterator-aware
        iteratorContext._coroutineStateMachineBox = iteratorContext._iteratorAgnosticCoroutineContext.ResultStateMachine as ICoroutineStateMachineBox;
        context._keyedServices = context.KeyedServices.Merge([new(AsyncIterator.s_asyncIteratorKey, iteratorContext._coroutineContextService)]);
    }

    private void BequestCoroutineContext(AsyncIteratorContext iteratorContext, AsyncIteratorContextService contextService, out bool isCoroutineCompleted)
    {
        var scope = new CoroutineScope();
        var context = new CoroutineContext();
        context._keyedServicesToBequest = ImmutableDictionary.CreateRange<Key, object>([new(CoroutineScope.s_coroutineScopeKey, scope)]);
        context._bequesterOrigin = CoroutineContextBequesterOrigin.ContextBequester;
        context._bequestContext = OnBequestCoroutineContext;
        ref var coroutineAwaiter = ref iteratorContext._coroutineAwaiter;
        CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref context);
        isCoroutineCompleted = coroutineAwaiter.IsCompleted;
    }

    private AsyncIteratorContext GetIteratorContext(out bool isCoroutineCompleted)
    {
        var iteratorContext = _iteratorContext;

        if (iteratorContext is not null) {
            isCoroutineCompleted = false;
            return iteratorContext;
        }

        iteratorContext = new AsyncIteratorContext(new AsyncIteratorContextService(AsyncIteratorContextServiceOperation.RequiresSupply));
        ref var coroutineAwaiter = ref iteratorContext._coroutineAwaiter;
        coroutineAwaiter = _coroutine.GetAwaiter();
        isCoroutineCompleted = coroutineAwaiter.IsCompleted;
        _iteratorContext = iteratorContext;
        if (!isCoroutineCompleted) {
            BequestCoroutineContext(iteratorContext, iteratorContext._coroutineContextService, out isCoroutineCompleted);
        }
        return iteratorContext;
    }

    public object Current { get; } = null!;

    public async ValueTask<bool> MoveNextAsync()
    {
        var iteratorContext = GetIteratorContext(out var isCoroutineCompleted);
        var coroutineContextService = iteratorContext._coroutineContextService;

        if (isCoroutineCompleted) {
            return false;
        }

        if (_nextOperation is { } nextOperation) {
            _nextOperation = null;

            if ((nextOperation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied) != 0) {
                var argumentsReceiver = new CoroutineArgumentReceiver(ref iteratorContext._iteratorAgnosticCoroutineContext);
                Debug.Assert(nextOperation.ArgumentKey is not null);
                Debug.Assert(nextOperation.Argument is not null);
                Debug.Assert(nextOperation.ArgumentCompletionSource is not null);
                argumentsReceiver.ReceiveCallableArgument(nextOperation.ArgumentKey, nextOperation.Argument, nextOperation.ArgumentCompletionSource);
                coroutineContextService.RequiresSupply();
                await nextOperation.AwaiterCompletionNotifier;
                iteratorContext._coroutineStateMachineBox?.MoveNext();

                if (iteratorContext._coroutineAwaiter.IsCompleted) {
                    return false;
                }
            } else {
                throw new NotImplementedException($"The next operation handling is not implemented: {nextOperation}");
            }
        }

        if ((coroutineContextService.CurrentOperation.State & AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied) == 0) {
            if (iteratorContext.HasCoroutineStateMachineBox) {
                throw new InvalidOperationException("Altough the coroutine is managed by a state machine, the coroutine misbehaved fatally by not supplying the next awaiter completion notifier");
            }
            return false;
        }

        while (coroutineContextService.CurrentOperation.State == AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied) {
            coroutineContextService.RequiresSupply();
            await coroutineContextService.CurrentOperation.AwaiterCompletionNotifier;
            iteratorContext._coroutineStateMachineBox?.MoveNext();
        };

        if ((coroutineContextService.CurrentOperation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied) != 0) {
            if ((coroutineContextService.CurrentOperation.State & AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied) == 0) {
                throw new InvalidOperationException("Altough the coroutine yielded an argument successfully, the coroutine misbehaved fatally by not supplying the next awaiter completion notifier");
            }

            _nextOperation = coroutineContextService.CurrentOperation;
            return true;

        }

        return false;
    }

    public void YieldReturn<TYieldResult>(TYieldResult result) => throw new NotImplementedException();
    public void Return() => throw new NotImplementedException();
    public void Return(TReturnResult result) => throw new NotImplementedException();
    public void Throw(Exception e) => throw new NotImplementedException();

    Coroutine<TReturnResult>.CoroutineAwaiter IAsyncIterator<TReturnResult>.GetAwaiter() => _coroutine.GetAwaiter();

    ConfiguredAwaitableCoroutine<TReturnResult> ConfigureAwait(bool continueOnCapturedContext) => _coroutine.ConfigureAwait(continueOnCapturedContext);

    private class AsyncIteratorContext
    {
        public readonly AsyncIteratorContextService _coroutineContextService;
        public Coroutine<TReturnResult>.CoroutineAwaiter _coroutineAwaiter;
        public CoroutineContext _iteratorAgnosticCoroutineContext;
        public ICoroutineStateMachineBox? _coroutineStateMachineBox;
        private bool? _hasCoroutineStateMachineBox;

        [MemberNotNullWhen(true, nameof(_coroutineStateMachineBox))]
        public bool HasCoroutineStateMachineBox {
            get => _hasCoroutineStateMachineBox ??= _coroutineStateMachineBox is not null;
        }

        public AsyncIteratorContext(AsyncIteratorContextService coroutineContextService) => _coroutineContextService = coroutineContextService;
    }
}
