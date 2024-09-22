using System.Diagnostics;
using Vernuntii.Coroutines.CompilerServices;

namespace Vernuntii.Coroutines.Iterators;

internal partial class AsyncIteratorImpl<TResult> : IAsyncIterator<TResult>, IAsyncIterator
{
    public bool IsCloneable => _isCloneable;

    public Key CurrentKey {
        get {
            EnsureNextOperationIsHavingSuppliedArgument();
            return _nextOperation._argumentKey;
        }
    }

    public object Current {
        get {
            EnsureNextOperationIsHavingSuppliedArgument();
            Debug.Assert(_nextOperation._argument is not null);
            return _nextOperation._argument;
        }
    }

    private readonly ICoroutineHolder _coroutineHolder;
    private readonly CoroutineContext _additiveContext;
    private readonly bool _isCloneable;
    private AsyncIteratorContext? _iteratorContext;
    private SuspensionPoint _nextOperation;

    internal AsyncIteratorImpl(Func<Coroutine> provider, in CoroutineContext additiveContext, bool isCloneable)
    {
        _coroutineHolder = new CoroutineHolder<Func<Coroutine>, Coroutine>(provider, isProvider: true, isGeneric: false);
        _additiveContext = additiveContext;
        _isCloneable = isCloneable;
    }

    internal AsyncIteratorImpl(Func<Coroutine<TResult>> provider, in CoroutineContext additiveContext, bool isCloneable)
    {
        _coroutineHolder = new CoroutineHolder<Func<Coroutine<TResult>>, Coroutine<TResult>>(provider, isProvider: true, isGeneric: true);
        _additiveContext = additiveContext;
        _isCloneable = isCloneable;
    }

    internal AsyncIteratorImpl(Coroutine coroutine, in CoroutineContext additiveContext, bool isCloneable)
    {
        _coroutineHolder = new CoroutineHolder<Coroutine, Coroutine>(coroutine, isProvider: false, isGeneric: false);
        _additiveContext = additiveContext;
        _isCloneable = isCloneable;
    }

    internal AsyncIteratorImpl(Coroutine<TResult> coroutine, in CoroutineContext additiveContext, bool isCloneable)
    {
        _coroutineHolder = new CoroutineHolder<Coroutine<TResult>, Coroutine<TResult>>(coroutine, isProvider: false, isGeneric: true);
        _additiveContext = additiveContext;
        _isCloneable = isCloneable;
    }

    private void OnBequestCoroutineContext(ref CoroutineContext context, in CoroutineContext contextToBequest)
    {
        var iteratorContext = GetIteratorContext(out _);
        context.InheritContext(in contextToBequest);

        // In case of WithContext, we want:
        //      1. allowing its providing coroutine to inherit context
        //      2. to iterate that providing coroutine and not WithContext
        if ((context.BequesterOrigin & CoroutineContextBequesterOrigin.ContextBequester) != 0) {
            return;
        }

        context._bequestContext = null;
        iteratorContext._iteratorAgnosticCoroutineContext = context; // Copy befor making context async-iterator-aware
        iteratorContext._coroutineStateMachineHolder = iteratorContext._iteratorAgnosticCoroutineContext.ResultStateMachine as IAsyncIteratorStateMachineHolder<TResult>;
        context._keyedServices = context.KeyedServices.Merge(
            CoroutineContextServiceMap.CreateRange(1, iteratorContext._iteratorContextService, static (x, y) => x.Emplace(AsyncIterator.s_asyncIteratorKey, y)),
            forceNewInstance: true);
        context._isCoroutineAsyncIteratorSupplier = true;
    }

    private void BequestCoroutineContext(AsyncIteratorContext iteratorContext, AsyncIteratorContextService contextService, out bool isCoroutineCompleted)
    {
        var contextToBequest = _additiveContext;
        contextToBequest._bequesterOrigin = CoroutineContextBequesterOrigin.ContextBequester;
        contextToBequest._bequestContext = OnBequestCoroutineContext;
        ref var coroutineAwaiter = ref iteratorContext._coroutineAwaiter;
        CoroutineMethodBuilderCore.ActOnCoroutine(ref coroutineAwaiter, in contextToBequest);
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

        iteratorContext = new AsyncIteratorContext(new AsyncIteratorContextService(SuspensionPoint.AwaiterCompletionNotifierRequired, isAsyncIteratorCloneable: _isCloneable));
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
        if ((_nextOperation._state & SuspensionPointState.ArgumentSupplied) != 0) {
            return;
        }

        if (_nextOperation._state == 0) {
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

        if (_nextOperation._state != 0) {
            try {
                if ((_nextOperation._state & SuspensionPointState.ArgumentSupplied) != 0) {
                    var argumentReceiver = new CoroutineArgumentReceiver(in iteratorContext._iteratorAgnosticCoroutineContext);
                    Debug.Assert(_nextOperation._argument is not null);
                    Debug.Assert(_nextOperation._argumentCompletionSource is not null);
                    argumentReceiver.ReceiveCallableArgument(_nextOperation._argumentKey, _nextOperation._argument, _nextOperation._argumentCompletionSource);
                    iteratorContextService._currentSuspensionPoint.RequireAwaiterCompletionNotifier();
                    await _nextOperation._awaiterCompletionNotifier;
                    iteratorContext._coroutineStateMachineHolder?.MoveNext();

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

        if ((iteratorContextService._currentSuspensionPoint._state & SuspensionPointState.AwaiterCompletionNotifierSupplied) == 0) {
            if (iteratorContext.HasCoroutineStateMachineBox) {
                throw new InvalidOperationException("Altough the underlying coroutine is managed by a state machine, the coroutine misbehaved fatally by not supplying the next awaiter completion notifier");
            }
            return false;
        }

        while (iteratorContextService._currentSuspensionPoint._state == SuspensionPointState.AwaiterCompletionNotifierSupplied) {
            iteratorContextService._currentSuspensionPoint.RequireAwaiterCompletionNotifier();
            await iteratorContextService._currentSuspensionPoint._awaiterCompletionNotifier;
            iteratorContext._coroutineStateMachineHolder?.MoveNext();
        };

        if ((iteratorContextService._currentSuspensionPoint._state & SuspensionPointState.ArgumentSupplied) != 0) {
            if ((iteratorContextService._currentSuspensionPoint._state & SuspensionPointState.AwaiterCompletionNotifierSupplied) == 0) {
                throw new InvalidOperationException("Altough the underlying coroutine yielded an argument successfully, the coroutine misbehaved fatally by not supplying the next awaiter completion notifier");
            }

            _nextOperation = iteratorContextService._currentSuspensionPoint;
            return true;
        }

        return false;
    }

    public void YieldReturn<TYieldResult>(TYieldResult result)
    {
        if (_nextOperation._state != 0) {
            try {
                var iteratorContext = GetIteratorContext(out _);

                if ((_nextOperation._state & SuspensionPointState.ArgumentSupplied) != 0) {
                    iteratorContext._iteratorContextService._currentSuspensionPoint.RequireAwaiterCompletionNotifier();
                    Debug.Assert(_nextOperation._argumentCompletionSource is not null);
                    _nextOperation._argumentCompletionSource.SetResult(result);
                    iteratorContext._coroutineStateMachineHolder?.MoveNext();
                }
            } finally {
                _nextOperation.Uninitialize();
            }
        } else {
            throw Exceptions.NotStartedAlreadyFinishedOrNotSuspended();
        }
    }

    public void Return(TResult result)
    {
        if (_nextOperation._state != 0) {
            try {
                var iteratorContext = GetIteratorContext(out _);

                if ((_nextOperation._state & SuspensionPointState.ArgumentSupplied) != 0) {
                    var completionSource = ManualResetValueTaskCompletionSource<TResult>.RentFromCache();
                    iteratorContext._iteratorContextService._currentSuspensionPoint.RequireAwaiterCompletionNotifier();
                    iteratorContext._coroutineStateMachineHolder!.SetAsyncIteratorCompletionSource(completionSource);
                    Debug.Assert(_nextOperation._argumentCompletionSource is not null);
                    _nextOperation._argumentCompletionSource.SetException(new CancellationException());
                    iteratorContext._coroutineStateMachineHolder?.MoveNext();
                    if (completionSource.GetStatus(completionSource.Version) == System.Threading.Tasks.Sources.ValueTaskSourceStatus.Faulted) {
                        iteratorContext._coroutineStateMachineHolder!.SetResult(result);
                    }
                    iteratorContext._coroutineStateMachineHolder!.SetAsyncIteratorCompletionSource(null);
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
        if (_nextOperation._state != 0) {
            try {
                var iteratorContext = GetIteratorContext(out _);

                if ((_nextOperation._state & SuspensionPointState.ArgumentSupplied) != 0) {
                    iteratorContext._iteratorContextService._currentSuspensionPoint.RequireAwaiterCompletionNotifier();
                    Debug.Assert(_nextOperation._argumentCompletionSource is not null);
                    _nextOperation._argumentCompletionSource.SetException(e);
                    iteratorContext._coroutineStateMachineHolder?.MoveNext();
                }
            } finally {
                _nextOperation.Uninitialize();
            }
        } else {
            throw Exceptions.NotStartedAlreadyFinishedOrNotSuspended();
        }
    }

    public TResult GetResult()
    {
        var iteratorContext = GetIteratorContext(out var isCoroutineCompleted);

        if (!isCoroutineCompleted) {
            throw new InvalidOperationException("The underlying coroutine has not finished yet");
        }

        return iteratorContext._coroutineAwaiter.GetResult();
    }

    public Coroutine<TResult> GetResultAsync()
    {
        var iteratorContext = GetIteratorContext(out var isCoroutineCompleted);

        if (isCoroutineCompleted) {
            goto exit;
        }

        var coroutineContextSerivce = iteratorContext._iteratorContextService;
        var nextOperation = _nextOperation;

        if (nextOperation._state == 0) {
            var currentOperation = iteratorContext._iteratorContextService._currentSuspensionPoint;
            var currentOperationState = currentOperation._state;

            if (currentOperationState == SuspensionPointState.AwaiterCompletionNotifierRequired) {
                goto exit;
            }

            if ((currentOperationState & (SuspensionPointState.ArgumentSupplied | SuspensionPointState.AwaiterCompletionNotifierSupplied)) != 0) {
                nextOperation = currentOperation;
                if (iteratorContext.HasCoroutineStateMachineBox) {
                    iteratorContext._coroutineStateMachineHolder.CoroutineContext._isCoroutineAsyncIteratorSupplier = false;
                }
                coroutineContextSerivce._currentSuspensionPoint.RequireAwaiterCompletionNotifier();
            } else {
                throw new InvalidOperationException($"The underlying coroutine cannot finish due to the unrecoverable state: {Enum.GetName(currentOperationState)}");
            }
        }

        if ((nextOperation._state & SuspensionPointState.ArgumentSupplied) != 0) {
            var argumentReceiver = new CoroutineArgumentReceiver(in iteratorContext._iteratorAgnosticCoroutineContext);
            Debug.Assert(nextOperation._argument is not null);
            Debug.Assert(nextOperation._argumentCompletionSource is not null);
            argumentReceiver.ReceiveCallableArgument(nextOperation._argumentKey, nextOperation._argument, nextOperation._argumentCompletionSource);
        }

        if ((nextOperation._state & SuspensionPointState.AwaiterCompletionNotifierSupplied) != 0) {
            var awaiterCompletionNotifierAwaiter = nextOperation._awaiterCompletionNotifier.ConfigureAwait(false).GetAwaiter();
            awaiterCompletionNotifierAwaiter.UnsafeOnCompleted(() => {
                // Allow the completion notifier to be returned to the pool
                awaiterCompletionNotifierAwaiter.GetResult(); // Should never throw
                iteratorContext._coroutineStateMachineHolder?.MoveNext();
            });
        }

        exit:
        _coroutineHolder.Coroutine.MarkCoroutineAsActedOn();
        return _coroutineHolder.Coroutine;
    }

    IAsyncIterator IAsyncIterator.Clone()
    {
        if (!_isCloneable) {
            throw new NotSupportedException();
        }

        if (_nextOperation._state != 0) {
            var ourIteratorContext = GetIteratorContext(out _);
            Debug.Assert(ourIteratorContext._coroutineStateMachineHolder is not null);
            IAsyncIteratorStateMachineHolder ourStateMachineHolder = ourIteratorContext._coroutineStateMachineHolder;
            var theirIteratorContextService = new AsyncIteratorContextService(in SuspensionPoint.AwaiterCompletionNotifierRequired, isAsyncIteratorCloneable: true);
            var theirStateMachineHolder = ourStateMachineHolder.CreateNewByCloningUnderlyingStateMachine(in _nextOperation, ref theirIteratorContextService._currentSuspensionPoint);
            theirStateMachineHolder.CoroutineContext = ourIteratorContext._iteratorAgnosticCoroutineContext;
            var theirIterator = new AsyncIteratorImpl<Nothing>(new Coroutine(theirStateMachineHolder, theirStateMachineHolder.Version), _additiveContext, isCloneable: true);
            { // Initialize their iterator
                var theirIteratorContext = new AsyncIteratorImpl<Nothing>.AsyncIteratorContext(theirIteratorContextService) {
                    _iteratorAgnosticCoroutineContext = ourIteratorContext._iteratorAgnosticCoroutineContext,
                    _coroutineStateMachineHolder = theirStateMachineHolder,
                    _coroutineAwaiter = theirIterator._coroutineHolder.Coroutine.GetAwaiter()
                };
                theirIterator._iteratorContext = theirIteratorContext;
                var asyncIteratorSpecificCoroutineContext = ourIteratorContext._iteratorAgnosticCoroutineContext;
                asyncIteratorSpecificCoroutineContext._keyedServices = asyncIteratorSpecificCoroutineContext.KeyedServices.Merge(
                    CoroutineContextServiceMap.CreateRange(1, theirIteratorContext._iteratorContextService, static (x, y) => x.Emplace(AsyncIterator.s_asyncIteratorKey, y)),
                    forceNewInstance: true);
                asyncIteratorSpecificCoroutineContext._isCoroutineAsyncIteratorSupplier = true;
                theirStateMachineHolder.CoroutineContext = asyncIteratorSpecificCoroutineContext;
            }
            return theirIterator;
        } else {
            throw Exceptions.NotStartedAlreadyFinishedOrNotSuspended();
        }
    }

    [SuppressMessage("Style", "IDE0007:Use implicit type", Justification = "Clarification")]
    IAsyncIterator<TResult> IAsyncIterator<TResult>.Clone()
    {
        if (!_isCloneable) {
            throw new NotSupportedException();
        }

        if (_nextOperation._state != 0) {
            var ourIteratorContext = GetIteratorContext(out _);
            Debug.Assert(ourIteratorContext._coroutineStateMachineHolder is not null);
            IAsyncIteratorStateMachineHolder<TResult> ourStateMachineHolder = ourIteratorContext._coroutineStateMachineHolder;
            var theirIteratorContextService = new AsyncIteratorContextService(in SuspensionPoint.AwaiterCompletionNotifierRequired, isAsyncIteratorCloneable: true);
            var theirStateMachineHolder = ourStateMachineHolder.CreateNewByCloningUnderlyingStateMachine(in _nextOperation, ref theirIteratorContextService._currentSuspensionPoint);
            theirStateMachineHolder.CoroutineContext = ourIteratorContext._iteratorAgnosticCoroutineContext;
            var theirIterator = new AsyncIteratorImpl<TResult>(new Coroutine<TResult>(theirStateMachineHolder, theirStateMachineHolder.Version), _additiveContext, isCloneable: true);
            { // Initialize their iterator
                var theirIteratorContext = new AsyncIteratorContext(theirIteratorContextService) {
                    _iteratorAgnosticCoroutineContext = ourIteratorContext._iteratorAgnosticCoroutineContext,
                    _coroutineStateMachineHolder = theirStateMachineHolder,
                    _coroutineAwaiter = theirIterator._coroutineHolder.Coroutine.GetAwaiter()
                };
                theirIterator._iteratorContext = theirIteratorContext;
                var asyncIteratorSpecificCoroutineContext = ourIteratorContext._iteratorAgnosticCoroutineContext;
                asyncIteratorSpecificCoroutineContext._keyedServices = asyncIteratorSpecificCoroutineContext.KeyedServices.Merge(
                    CoroutineContextServiceMap.CreateRange(1, theirIteratorContext._iteratorContextService, static (x, y) => x.Emplace(AsyncIterator.s_asyncIteratorKey, y)),
                    forceNewInstance: true);
                asyncIteratorSpecificCoroutineContext._isCoroutineAsyncIteratorSupplier = true;
                theirStateMachineHolder.CoroutineContext = asyncIteratorSpecificCoroutineContext;
            }
            return theirIterator;
        } else {
            throw Exceptions.NotStartedAlreadyFinishedOrNotSuspended();
        }
    }

    private static class Exceptions
    {
        public static NotImplementedException NextOperationHandlingNotImplemented(SuspensionPoint nextOperation) =>
            new($"The next operation handling is not implemented: {nextOperation}");

        public static InvalidOperationException ExpectedSuppliedAwaiterCompletionNotifier() =>
            new("Altough the underlying coroutine yielded an argument successfully, the coroutine misbehaved fatally by not supplying the next awaiter completion notifier");

        public static InvalidOperationException NotStartedAlreadyFinishedOrNotSuspended() =>
            new("The iterator has not started, has already finished or is not suspended");
    }

    private class AsyncIteratorContext
    {
        public readonly AsyncIteratorContextService _iteratorContextService;
        public CoroutineAwaiter<TResult> _coroutineAwaiter;
        public CoroutineContext _iteratorAgnosticCoroutineContext;
        public IAsyncIteratorStateMachineHolder<TResult>? _coroutineStateMachineHolder;
        private bool? _hasCoroutineStateMachineBox;

        [MemberNotNullWhen(true, nameof(_coroutineStateMachineHolder))]
        public bool HasCoroutineStateMachineBox {
            get => _hasCoroutineStateMachineBox ??= _coroutineStateMachineHolder is not null;
        }

        public AsyncIteratorContext(AsyncIteratorContextService iteratorContextService) => _iteratorContextService = iteratorContextService;
    }

    private interface ICoroutineHolder
    {
        bool IsUnderlyingCoroutineGeneric { get; }

        ref Coroutine<TResult> Coroutine { get; }
    }

    private class CoroutineHolder<TCoroutineProvider, TCoroutine> : ICoroutineHolder
        where TCoroutineProvider : notnull
    {
        TCoroutineProvider _coroutineProvider;
        Coroutine<TResult> _coroutine;
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

        ref Coroutine<TResult> ICoroutineHolder.Coroutine {
            get {
                if (!_isDeclared) {
                    if (_isProvider) {
                        var coroutineProvider = Unsafe.As<Func<TCoroutine>>(_coroutineProvider);
                        var coroutine = coroutineProvider();
                        _coroutine = Unsafe.As<TCoroutine, Coroutine<TResult>>(ref coroutine);
                    } else {
                        _coroutine = Unsafe.As<TCoroutineProvider, Coroutine<TResult>>(ref _coroutineProvider);
                    }

                    _isDeclared = true;
                }

                return ref _coroutine;
            }
        }
    }
}
