using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vernuntii.Coroutines.CompilerServices;

namespace Vernuntii.Coroutines.Iterators;

internal partial class AsyncIteratorImpl<TReturnResult> : IAsyncIterator<TReturnResult>, IAsyncIterator
{
    public Key CurrentKey {
        get {
            EnsureNextOperationIsHavingSuppliedArgument();
            return _nextOperation.ArgumentKey;
        }
    }

    public object Current {
        get {
            EnsureNextOperationIsHavingSuppliedArgument();
            Debug.Assert(_nextOperation.Argument is not null);
            return _nextOperation.Argument;
        }
    }

    private readonly ICoroutineHolder _coroutineHolder;
    private AsyncIteratorContext? _iteratorContext;
    private AsyncIteratorContextServiceOperation _nextOperation;
    private List<KeyValuePair<Key, object>>? _yieldHistory;

    public AsyncIteratorImpl(Func<Coroutine> provider)
    {
        _coroutineHolder = new CoroutineHolder<Func<Coroutine>, Coroutine>(provider, isProvider: true, isGeneric: false);
    }

    public AsyncIteratorImpl(Func<Coroutine<TReturnResult>> provider)
    {
        _coroutineHolder = new CoroutineHolder<Func<Coroutine<TReturnResult>>, Coroutine<TReturnResult>>(provider, isProvider: true, isGeneric: true);
    }

    public AsyncIteratorImpl(Coroutine coroutine)
    {
        _coroutineHolder = new CoroutineHolder<Coroutine, Coroutine>(coroutine, isProvider: false, isGeneric: false);
    }

    public AsyncIteratorImpl(Coroutine<TReturnResult> coroutine)
    {
        _coroutineHolder = new CoroutineHolder<Coroutine<TReturnResult>, Coroutine<TReturnResult>>(coroutine, isProvider: false, isGeneric: true);
    }

    private void OnBequestCoroutineContext(ref CoroutineContext context, in CoroutineContext contextToBequest)
    {
        var iteratorContext = GetIteratorContext(out _);
        context.InheritContext(in contextToBequest);
        context._bequestContext = null;
        iteratorContext._iteratorAgnosticCoroutineContext = context; // Copy befor making context async-iterator-aware
        iteratorContext._coroutineStateMachineBox = iteratorContext._iteratorAgnosticCoroutineContext.ResultStateMachine as IAsyncIteratorStateMachineBox<TReturnResult>;
        context._keyedServices = context.KeyedServices.Merge(
            CoroutineContextServiceMap.CreateRange(1, iteratorContext._iteratorContextService, static (x, y) => x.Emplace(AsyncIterator.s_asyncIteratorKey, y)),
            forceNewInstance: true);
        context._isCoroutineAsyncIteratorSupplier = true;
    }

    private void BequestCoroutineContext(AsyncIteratorContext iteratorContext, AsyncIteratorContextService contextService, out bool isCoroutineCompleted)
    {
        var scope = new CoroutineScope();
        var context = new CoroutineContext();
        context._keyedServicesToBequest = CoroutineContextServiceMap.CreateRange(1, scope, static (x, y) => x.Emplace(CoroutineScope.s_coroutineScopeKey, y));
        context._bequesterOrigin = CoroutineContextBequesterOrigin.ContextBequester;
        context._bequestContext = OnBequestCoroutineContext;
        ref var coroutineAwaiter = ref iteratorContext._coroutineAwaiter;
        CoroutineMethodBuilderCore.ActOnCoroutine(ref coroutineAwaiter, ref context);
        isCoroutineCompleted = coroutineAwaiter.IsCompleted;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private AsyncIteratorContext GetIteratorContext(out bool isCoroutineCompleted)
    {
        var iteratorContext = _iteratorContext;

        if (iteratorContext is not null) {
            isCoroutineCompleted = iteratorContext._coroutineAwaiter.IsCompleted;
            return iteratorContext;
        }

        iteratorContext = new AsyncIteratorContext(new AsyncIteratorContextService(AsyncIteratorContextServiceOperation.AwaiterCompletionNotifierRequired));
        ref var coroutineAwaiter = ref iteratorContext._coroutineAwaiter;
        coroutineAwaiter = _coroutineHolder.Coroutine.GetAwaiter();
        isCoroutineCompleted = coroutineAwaiter.IsCompleted;
        _iteratorContext = iteratorContext;

        if (!isCoroutineCompleted) {
            BequestCoroutineContext(iteratorContext, iteratorContext._iteratorContextService, out isCoroutineCompleted);
        }

        return iteratorContext;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureNextOperationIsHavingSuppliedArgument()
    {
        // hot-path
        if ((_nextOperation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied) != 0) {
            return;
        }

        if (_nextOperation.State == 0) {
            throw Exceptions.NotStartedAlreadyFinishedOrNotSuspended();
        }

        throw new InvalidOperationException("Although the iterator is suspended, the coroutine yielder which let to the suspension misbehaved fatally by not supplying an argument");
    }

    public async ValueTask<bool> MoveNextAsync()
    {
        var iteratorContext = GetIteratorContext(out var isCoroutineCompleted);
        var iteratorContextService = iteratorContext._iteratorContextService;

        if (isCoroutineCompleted) {
            return false;
        }

        if (_nextOperation.State != 0) {
            try {
                if ((_nextOperation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied) != 0) {
                    var argumentReceiver = new CoroutineArgumentReceiver(ref iteratorContext._iteratorAgnosticCoroutineContext);
                    Debug.Assert(_nextOperation.Argument is not null);
                    Debug.Assert(_nextOperation.ArgumentCompletionSource is not null);
                    argumentReceiver.ReceiveCallableArgument(_nextOperation.ArgumentKey, _nextOperation.Argument, _nextOperation.ArgumentCompletionSource);
                    iteratorContextService.CurrentOperation.RequireAwaiterCompletionNotifier();
                    await _nextOperation.AwaiterCompletionNotifier;
                    iteratorContext._coroutineStateMachineBox?.MoveNext();

                    if (iteratorContext._coroutineAwaiter.IsCompleted) {
                        return false;
                    }
                } else {
                    throw Exceptions.NextOperationHandlingNotImplemented(_nextOperation);
                }
            } finally {
                _nextOperation.Uninitialize();
            }
        }

        if ((iteratorContextService.CurrentOperation.State & AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied) == 0) {
            if (iteratorContext.HasCoroutineStateMachineBox) {
                throw new InvalidOperationException("Altough the underlying coroutine is managed by a state machine, the coroutine misbehaved fatally by not supplying the next awaiter completion notifier");
            }
            return false;
        }

        while (iteratorContextService.CurrentOperation.State == AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied) {
            iteratorContextService.CurrentOperation.RequireAwaiterCompletionNotifier();
            await iteratorContextService.CurrentOperation.AwaiterCompletionNotifier;
            iteratorContext._coroutineStateMachineBox?.MoveNext();
        };

        if ((iteratorContextService.CurrentOperation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied) != 0) {
            if ((iteratorContextService.CurrentOperation.State & AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied) == 0) {
                throw new InvalidOperationException("Altough the underlying coroutine yielded an argument successfully, the coroutine misbehaved fatally by not supplying the next awaiter completion notifier");
            }

            _nextOperation = iteratorContextService.CurrentOperation;
            return true;
        }

        return false;
    }

    public void YieldReturn<TYieldResult>(TYieldResult result)
    {
        if (_nextOperation.State != 0) {
            try {
                var iteratorContext = GetIteratorContext(out _);

                if ((_nextOperation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied) != 0) {
                    iteratorContext._iteratorContextService.CurrentOperation.RequireAwaiterCompletionNotifier();
                    Debug.Assert(_nextOperation.ArgumentCompletionSource is not null);
                    _nextOperation.ArgumentCompletionSource.SetResult(result);
                    iteratorContext._coroutineStateMachineBox?.MoveNext();
                }
            } finally {
                _nextOperation.Uninitialize();
            }
        } else {
            throw Exceptions.NotStartedAlreadyFinishedOrNotSuspended();
        }
    }

    public void Return(TReturnResult result)
    {
        if (_nextOperation.State != 0) {
            try {
                var iteratorContext = GetIteratorContext(out _);

                if ((_nextOperation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied) != 0) {
                    var completionSource = ManualResetValueTaskCompletionSource<TReturnResult>.RentFromCache();
                    iteratorContext._iteratorContextService.CurrentOperation.RequireAwaiterCompletionNotifier();
                    iteratorContext._coroutineStateMachineBox!.SetAsyncIteratorCompletionSource(completionSource);
                    Debug.Assert(_nextOperation.ArgumentCompletionSource is not null);
                    _nextOperation.ArgumentCompletionSource.SetException(new CancellationException());
                    iteratorContext._coroutineStateMachineBox?.MoveNext();
                    if (completionSource.GetStatus(completionSource.Version) == System.Threading.Tasks.Sources.ValueTaskSourceStatus.Faulted) {
                        iteratorContext._coroutineStateMachineBox!.SetResult(result);
                    }
                    iteratorContext._coroutineStateMachineBox!.SetAsyncIteratorCompletionSource(null);
                }
            } finally {
                _nextOperation.Uninitialize();
            }
        } else {
            throw Exceptions.NotStartedAlreadyFinishedOrNotSuspended();
        }
    }

    public void Throw(Exception e)
    {
        if (_nextOperation.State != 0) {
            try {
                var iteratorContext = GetIteratorContext(out _);

                if ((_nextOperation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied) != 0) {
                    iteratorContext._iteratorContextService.CurrentOperation.RequireAwaiterCompletionNotifier();
                    Debug.Assert(_nextOperation.ArgumentCompletionSource is not null);
                    _nextOperation.ArgumentCompletionSource.SetException(e);
                    iteratorContext._coroutineStateMachineBox?.MoveNext();
                }
            } finally {
                _nextOperation.Uninitialize();
            }
        } else {
            throw Exceptions.NotStartedAlreadyFinishedOrNotSuspended();
        }
    }

    public TReturnResult GetResult()
    {
        var iteratorContext = GetIteratorContext(out var isCoroutineCompleted);

        if (!isCoroutineCompleted) {
            throw new InvalidOperationException("The underlying coroutine has not finished yet");
        }

        return iteratorContext._coroutineAwaiter.GetResult();
    }

    public Coroutine<TReturnResult> GetResultAsync()
    {
        var iteratorContext = GetIteratorContext(out var isCoroutineCompleted);

        if (isCoroutineCompleted) {
            goto exit;
        }

        var coroutineContextSerivce = iteratorContext._iteratorContextService;
        var nextOperation = _nextOperation;

        if (nextOperation.State == 0) {
            var currentOperation = iteratorContext._iteratorContextService.CurrentOperation;
            var currentOperationState = currentOperation.State;

            if (currentOperationState == AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierRequired) {
                goto exit;
            }

            if ((currentOperationState & (AsyncIteratorContextServiceOperationState.ArgumentSupplied | AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied)) != 0) {
                nextOperation = currentOperation;
                if (iteratorContext.HasCoroutineStateMachineBox) {
                    iteratorContext._coroutineStateMachineBox.CoroutineContext._isCoroutineAsyncIteratorSupplier = false;
                }
                coroutineContextSerivce.CurrentOperation.RequireAwaiterCompletionNotifier();
            } else {
                throw new InvalidOperationException($"The underlying coroutine cannot finish due to the unrecoverable state: {Enum.GetName(currentOperationState)}");
            }
        }

        if ((nextOperation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied) != 0) {
            var argumentReceiver = new CoroutineArgumentReceiver(ref iteratorContext._iteratorAgnosticCoroutineContext);
            Debug.Assert(nextOperation.Argument is not null);
            Debug.Assert(nextOperation.ArgumentCompletionSource is not null);
            argumentReceiver.ReceiveCallableArgument(nextOperation.ArgumentKey, nextOperation.Argument, nextOperation.ArgumentCompletionSource);
        }

        if ((nextOperation.State & AsyncIteratorContextServiceOperationState.AwaiterCompletionNotifierSupplied) != 0) {
            var awaiterCompletionNotifierAwaiter = nextOperation.AwaiterCompletionNotifier.ConfigureAwait(false).GetAwaiter();
            awaiterCompletionNotifierAwaiter.UnsafeOnCompleted(() => {
                // Allow the completion notifier to be returned to the pool
                awaiterCompletionNotifierAwaiter.GetResult(); // Should never throw
                iteratorContext._coroutineStateMachineBox?.MoveNext();
            });
        }

        exit:
        _coroutineHolder.Coroutine.MarkCoroutineAsActedOn();
        return _coroutineHolder.Coroutine;
    }

    private static class Exceptions
    {
        public static NotImplementedException NextOperationHandlingNotImplemented(AsyncIteratorContextServiceOperation nextOperation) =>
            new($"The next operation handling is not implemented: {nextOperation}");

        public static InvalidOperationException ExpectedSuppliedAwaiterCompletionNotifier() =>
            new("Altough the underlying coroutine yielded an argument successfully, the coroutine misbehaved fatally by not supplying the next awaiter completion notifier");

        public static InvalidOperationException NotStartedAlreadyFinishedOrNotSuspended() =>
            new("The iterator has not started, has already finished or is not suspended");
    }

    private class AsyncIteratorContext
    {
        public readonly AsyncIteratorContextService _iteratorContextService;
        public CoroutineAwaiter<TReturnResult> _coroutineAwaiter;
        public CoroutineContext _iteratorAgnosticCoroutineContext;
        public IAsyncIteratorStateMachineBox<TReturnResult>? _coroutineStateMachineBox;
        private bool? _hasCoroutineStateMachineBox;

        [MemberNotNullWhen(true, nameof(_coroutineStateMachineBox))]
        public bool HasCoroutineStateMachineBox {
            get => _hasCoroutineStateMachineBox ??= _coroutineStateMachineBox is not null;
        }

        public AsyncIteratorContext(AsyncIteratorContextService iteratorContextService) => _iteratorContextService = iteratorContextService;
    }

    private interface ICoroutineHolder
    {
        bool IsUnderlyingCoroutineGeneric { get; }

        ref Coroutine<TReturnResult> Coroutine { get; }
    }

    private class CoroutineHolder<TCoroutineProvider, TCoroutine> : ICoroutineHolder
        where TCoroutineProvider : notnull
    {
        TCoroutineProvider _coroutineProvider;
        Coroutine<TReturnResult> _coroutine;
        readonly bool _isProvider;
        readonly bool _isGeneric;
        bool _isDeclared;

        bool ICoroutineHolder.IsUnderlyingCoroutineGeneric => _isGeneric;

        public CoroutineHolder(TCoroutineProvider coroutineProvider, bool isProvider, bool isGeneric)
        {
            _coroutineProvider = coroutineProvider;
            _isProvider = isProvider;
            _isGeneric = isGeneric;
        }

        ref Coroutine<TReturnResult> ICoroutineHolder.Coroutine {
            get {
                if (!_isDeclared) {
                    if (_isProvider) {
                        var coroutineProvider = Unsafe.As<Func<TCoroutine>>(_coroutineProvider);
                        var coroutine = coroutineProvider();
                        _coroutine = Unsafe.As<TCoroutine, Coroutine<TReturnResult>>(ref coroutine);
                    } else {
                        _coroutine = Unsafe.As<TCoroutineProvider, Coroutine<TReturnResult>>(ref _coroutineProvider);
                    }

                    _isDeclared = true;
                }

                return ref _coroutine;
            }
        }
    }
}
