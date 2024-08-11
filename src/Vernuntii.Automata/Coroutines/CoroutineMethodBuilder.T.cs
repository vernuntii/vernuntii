using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

//[StructLayout(LayoutKind.Auto)]
public partial struct CoroutineMethodBuilder<T>
{
    public static CoroutineMethodBuilder<T> Create()
    {
        return new CoroutineMethodBuilder<T>();
    }

    internal static CoroutineStateMachineBox CreateWeaklyTyedStateMachineBox() => new CoroutineStateMachineBox<IAsyncStateMachine>();

    /// <summary>Gets the "boxed" state machine object.</summary>
    /// <typeparam name="TStateMachine">Specifies the type of the async state machine.</typeparam>
    /// <param name="stateMachine">The state machine.</param>
    /// <param name="stateMachineBox">A reference to the field containing the initialized state machine box.</param>
    /// <returns>The "boxed" state machine.</returns>
    private static ICoroutineStateMachineBox GetStateMachineBox<TStateMachine>(ref TStateMachine stateMachine, [NotNull] ref CoroutineStateMachineBox? stateMachineBox)
        where TStateMachine : IAsyncStateMachine
    {
        ExecutionContext? currentContext = ExecutionContext.Capture();

        // Check first for the most common case: not the first yield in an async method.
        // In this case, the first yield will have already "boxed" the state machine in
        // a strongly-typed manner into an AsyncStateMachineBox.  It will already contain
        // the state machine as well as a MoveNextDelegate and a context.  The only thing
        // we might need to do is update the context if that's changed since it was stored.
        if (stateMachineBox is CoroutineStateMachineBox<TStateMachine> stronglyTypedBox) {
            if (stronglyTypedBox.Context != currentContext) {
                stronglyTypedBox.Context = currentContext;
            }

            return stronglyTypedBox;
        }

        // The least common case: we have a weakly-typed boxed.  This results if the debugger
        // or some other use of reflection accesses a property like ObjectIdForDebugger.  In
        // such situations, we need to get an object to represent the builder, but we don't yet
        // know the type of the state machine, and thus can't use TStateMachine.  Instead, we
        // use the IAsyncStateMachine interface, which all TStateMachines implement.  This will
        // result in a boxing allocation when storing the TStateMachine if it's a struct, but
        // this only happens in active debugging scenarios where such performance impact doesn't
        // matter.
        if (stateMachineBox is CoroutineStateMachineBox<IAsyncStateMachine> weaklyTypedBox) {
            // If this is the first await, we won't yet have a state machine, so store it.
            if (weaklyTypedBox.StateMachine is null) {
                Debugger.NotifyOfCrossThreadDependency(); // same explanation as with usage below
                weaklyTypedBox.StateMachine = stateMachine;
            }

            // Update the context.  This only happens with a debugger, so no need to spend
            // extra IL checking for equality before doing the assignment.
            weaklyTypedBox.Context = currentContext;
            return weaklyTypedBox;
        }

        // Alert a listening debugger that we can't make forward progress unless it slips threads.
        // If we don't do this, and a method that uses "await foo;" is invoked through funceval,
        // we could end up hooking up a callback to push forward the async method's state machine,
        // the debugger would then abort the funceval after it takes too long, and then continuing
        // execution could result in another callback being hooked up.  At that point we have
        // multiple callbacks registered to push the state machine, which could result in bad behavior.
        Debugger.NotifyOfCrossThreadDependency();

        // At this point, m_task should really be null, in which case we want to create the box.
        // However, in a variety of debugger-related (erroneous) situations, it might be non-null,
        // e.g. if the Task property is examined in a Watch window, forcing it to be lazily-initialized
        // as a Task<TResult> rather than as an ValueTaskStateMachineBox.  The worst that happens in such
        // cases is we lose the ability to properly step in the debugger, as the debugger uses that
        // object's identity to track this specific builder/state machine.  As such, we proceed to
        // overwrite whatever's there anyway, even if it's non-null.
        var typedStateMachineBox = CoroutineStateMachineBox<TStateMachine>.RentFromCache();
        stateMachineBox = typedStateMachineBox; // important: this must be done before storing stateMachine into box.StateMachine!
        typedStateMachineBox.StateMachine = stateMachine;
        typedStateMachineBox.Context = currentContext;
        return typedStateMachineBox;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)] // workaround boxing allocations in Tier0: https://github.com/dotnet/runtime/issues/9120
    internal static void AwaitUnsafeOnCompleted<TAwaiter>(
            ref TAwaiter awaiter, ICoroutineStateMachineBox stateMachineBox)
            where TAwaiter : ICriticalNotifyCompletion
    {
        if ((null != (object?)default(TAwaiter)) && (awaiter is ICoroutineStateMachineBoxAwareAwaiter)) {
            try {
                ((ICoroutineStateMachineBoxAwareAwaiter)awaiter).AwaitUnsafeOnCompleted(stateMachineBox);
            } catch (Exception e) {
                // Whereas with Task the code that hooks up and invokes the continuation is all local to corelib,
                // with ValueTaskAwaiter we may be calling out to an arbitrary implementation of IValueTaskSource
                // wrapped in the ValueTask, and as such we protect against errant exceptions that may emerge.
                // We don't want such exceptions propagating back into the async method, which can't handle
                // exceptions well at that location in the state machine, especially if the exception may occur
                // after the ValueTaskAwaiter already successfully hooked up the callback, in which case it's possible
                // two different flows of execution could end up happening in the same async method call.
                GlobalScope.ThrowAsync(e, targetContext: null);
            }
        } else {
            // The awaiter isn't specially known. Fall back to doing a normal await.
            try {
                awaiter.UnsafeOnCompleted(stateMachineBox.MoveNextAction);
            } catch (Exception e) {
                GlobalScope.ThrowAsync(e, targetContext: null);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine, [NotNull] ref CoroutineStateMachineBox? stateMachineBox)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
    {
        ICoroutineStateMachineBox box = GetStateMachineBox(ref stateMachine, ref stateMachineBox);
        CoroutineMethodBuilder<VoidTaskResult>.AwaitUnsafeOnCompleted(ref awaiter, box);
    }

    public unsafe Coroutine<T> Task {
        get {
            fixed (CoroutineMethodBuilder<T>* builder = &this) {
                _coroutineNode.SetResultStateMachine(new CoroutineResultStateMachine(builder));
                var stateMachineBox = _stateMachineBox ??= CreateWeaklyTyedStateMachineBox();
                return new Coroutine<T>(new ValueTask<T>(stateMachineBox, stateMachineBox.Version), builder);
            }
        }
    }

    private CoroutineStackNode _coroutineNode;
    private Action? _stateMachineInitiator;
    private CoroutineStateMachineBox _stateMachineBox;
    //private PoolingAsyncValueTaskMethodBuilder<T> _builder; // Must not be readonly due to mutable struct

    internal void SetCoroutineNode(ref CoroutineStackNode parentNode)
    {
        parentNode.InitializeChildCoroutine(ref _coroutineNode);
    }

    [DebuggerStepThrough]
    public unsafe void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        _stateMachineInitiator = stateMachine.MoveNext;
    }

    internal unsafe void Start()
    {
        _coroutineNode.Start();
        _stateMachineInitiator?.Invoke();
        _stateMachineInitiator = null;
    }

    public void SetException(Exception e)
    {
        var resultStateMachine = Unsafe.As<CoroutineResultStateMachine>(_coroutineNode.ResultStateMachine);
        resultStateMachine.SetException(e);
        _coroutineNode.Stop();
    }

    public void SetResult(T result)
    {
        var resultStateMachine = Unsafe.As<CoroutineResultStateMachine>(_coroutineNode.ResultStateMachine);
        resultStateMachine.SetResult(result);
        _coroutineNode.Stop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        try {
            awaiter.OnCompleted(GetStateMachineBox(ref stateMachine, ref _stateMachineBox).MoveNextAction);
        } catch (Exception e) {
            GlobalScope.ThrowAsync(e, targetContext: null);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        CoroutineMethodBuilderCore.ProcessAwaiterBeforeAwaitingOnCompleted(ref awaiter, ref _coroutineNode);
        AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine, ref _stateMachineBox);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        //_stateMachineBox.SetStateMachine(stateMachine);
    }

    internal unsafe class CoroutineResultStateMachine : AbstractCoroutineResultStateMachine<T>
    {
        private CoroutineMethodBuilder<T>* _builder;

        public CoroutineResultStateMachine(CoroutineMethodBuilder<T>* builder) => _builder = builder;

        protected override void SetExceptionCore(Exception error)
        {
            _builder->_stateMachineBox.SetException(error);
        }

        protected override void SetResultCore(T result)
        {
            _builder->_stateMachineBox.SetResult(result);
        }
    }
}
