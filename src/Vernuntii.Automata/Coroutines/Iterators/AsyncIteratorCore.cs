using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines.Iterators;

internal class AsyncIteratorCore<TReturnResult> : IAsyncIterator, IAsyncIterator<TReturnResult>
{
    private readonly ICoroutineHolder _coroutineHolder;
    private AsyncIteratorContext? _iteratorContext;
    private AsyncIteratorContextServiceOperation? _nextOperation;

    public AsyncIteratorCore(Func<Coroutine> provider)
    {
        _coroutineHolder = new CoroutineHolder<Func<Coroutine>, Coroutine>(provider, isProvider: true, isGeneric: false);
    }

    public AsyncIteratorCore(Func<Coroutine<TReturnResult>> provider)
    {
        _coroutineHolder = new CoroutineHolder<Func<Coroutine<TReturnResult>>, Coroutine<TReturnResult>>(provider, isProvider: true, isGeneric: true);
    }

    public AsyncIteratorCore(Coroutine coroutine)
    {
        _coroutineHolder = new CoroutineHolder<Coroutine, Coroutine>(coroutine, isProvider: false, isGeneric: false);
    }

    public AsyncIteratorCore(Coroutine<TReturnResult> coroutine)
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
        context._keyedServices = context.KeyedServices.Merge([new(AsyncIterator.s_asyncIteratorKey, iteratorContext._coroutineContextService)]);
        context._isAsyncIteratorSupplier = true;
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
            isCoroutineCompleted = iteratorContext._coroutineAwaiter.IsCompleted;
            return iteratorContext;
        }

        iteratorContext = new AsyncIteratorContext(new AsyncIteratorContextService(AsyncIteratorContextServiceOperation.RequiringAwaiterCompletionNotifier));
        ref var coroutineAwaiter = ref iteratorContext._coroutineAwaiter;
        coroutineAwaiter = _coroutineHolder.Coroutine.GetAwaiter();
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
                coroutineContextService.RequireAwaiterCompletionNotifier();
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
            coroutineContextService.RequireAwaiterCompletionNotifier();
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

    private void ReturnCore(TReturnResult result)
    {
        if (_nextOperation is { } nextOperation) {
            _nextOperation = null;
            var iteratorContext = GetIteratorContext(out _);

            if ((nextOperation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied) != 0) {
                var completionSource = ValueTaskCompletionSource<TReturnResult>.RentFromCache();
                iteratorContext._coroutineContextService.RequireAwaiterCompletionNotifier();
                iteratorContext._coroutineStateMachineBox!.SetAsyncIteratorCompletionSource(completionSource);
                Debug.Assert(nextOperation.ArgumentCompletionSource is not null);
                nextOperation.ArgumentCompletionSource.SetException(new CancellationException());
                iteratorContext._coroutineStateMachineBox?.MoveNext();
                if (completionSource.GetStatus(completionSource.Version) == System.Threading.Tasks.Sources.ValueTaskSourceStatus.Faulted) {
                    iteratorContext._coroutineStateMachineBox!.SetResult(result);
                }
                iteratorContext._coroutineStateMachineBox!.SetAsyncIteratorCompletionSource(null);
            }
        } else {
            throw new InvalidOperationException("The iterator has not started or has already finished");
        }
    }

    void IAsyncIterator.Return() => ReturnCore(default!);

    void IAsyncIterator<TReturnResult>.Return(TReturnResult result) => ReturnCore(result);

    public void Throw(Exception e)
    {
        if (_nextOperation is { } nextOperation) {
            _nextOperation = null;
            var iteratorContext = GetIteratorContext(out _);

            if ((nextOperation.State & AsyncIteratorContextServiceOperationState.ArgumentSupplied) != 0) {
                iteratorContext._coroutineContextService.RequireAwaiterCompletionNotifier();
                Debug.Assert(nextOperation.ArgumentCompletionSource is not null);
                nextOperation.ArgumentCompletionSource.SetException(new CancellationException());
                iteratorContext._coroutineStateMachineBox?.MoveNext();
            }
        } else {
            throw new InvalidOperationException("The iterator has not started or has already finished");
        }
    }

    TReturnResult IAsyncIterator<TReturnResult>.GetResult() => throw new NotImplementedException();
    void IAsyncIterator.GetResult() => throw new NotImplementedException();

    Coroutine<TReturnResult> IAsyncIterator<TReturnResult>.GetResultAsync() => throw new NotImplementedException();
    Coroutine IAsyncIterator.GetResultAsync() => throw new NotImplementedException();

    private class AsyncIteratorContext
    {
        public readonly AsyncIteratorContextService _coroutineContextService;
        public Coroutine<TReturnResult>.CoroutineAwaiter _coroutineAwaiter;
        public CoroutineContext _iteratorAgnosticCoroutineContext;
        public IAsyncIteratorStateMachineBox<TReturnResult>? _coroutineStateMachineBox;
        private bool? _hasCoroutineStateMachineBox;

        [MemberNotNullWhen(true, nameof(_coroutineStateMachineBox))]
        public bool HasCoroutineStateMachineBox {
            get => _hasCoroutineStateMachineBox ??= _coroutineStateMachineBox is not null;
        }

        public AsyncIteratorContext(AsyncIteratorContextService coroutineContextService) => _coroutineContextService = coroutineContextService;
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
