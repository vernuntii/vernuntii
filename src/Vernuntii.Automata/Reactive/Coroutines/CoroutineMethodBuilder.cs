using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Vernuntii.Reactive.Coroutines.Stepping;

namespace Vernuntii.Reactive.Coroutines;

public struct CoroutineMethodBuilder
{
    internal static int s_locker;
    internal static CoroutineSite? s_site;

    public static CoroutineMethodBuilder Create()
    {
        if (Interlocked.CompareExchange(ref s_locker, 2, 1) == 1) {
            retry:
            var site = s_site;
            while (Interlocked.CompareExchange(ref s_site, null, site) == null) {
                goto retry;
            }
            s_locker = 0;
            ArgumentNullException.ThrowIfNull(site);
            return new CoroutineMethodBuilder(site);
        }

        return new(null);
    }

    public Coroutine Task => new Coroutine(m_builder.Task, m_site);

    private AsyncTaskMethodBuilder m_builder;
    private CoroutineSite m_site;
    private Coroutine? m_task;
    private Action? m_firstContinuation;

    public CoroutineMethodBuilder(CoroutineSite? site)
    {
        m_builder = new();
        m_site = site!;
    }

    public void SetException(Exception e) => m_builder.SetException(e);

    public void SetResult()
    {
        m_builder.SetResult();
    }

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        m_builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
    }

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        var site = m_site;

        if (site != null) {
            if (awaiter is IStepCompletionHandler stepAcceptor) {
                m_site.IncomingStepChannel.Writer.TryWrite(stepAcceptor);

                //var continuationCollector = new ContinuationCollector();
                //m_builder.AwaitUnsafeOnCompleted(ref continuationCollector, ref stateMachine);
                //m_builder.AwaitUnsafeOnCompleted(ref continuationCollector, ref stateMachine);
                //var continautionWrapper = new ContinuationWrapper(continuationCollector.Continuation);
                //awaiter.UnsafeOnCompleted(continautionWrapper.Run);
            }
        }

        m_builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

        //if (awaiter is Coroutine.CoroutineAwaiter) {
        //    m_builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        //} else if (m_firstContinuation == null) {
        //    var continuationCollector = new ContinuationCollector();
        //    m_builder.AwaitUnsafeOnCompleted(ref continuationCollector, ref stateMachine);
        //    m_firstContinuation = continuationCollector.Continuation;
        //} else { 

        //}
    }

    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        m_builder.Start(ref stateMachine);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        m_builder.SetStateMachine(stateMachine);
    }

    private struct ContinuationCollector : ICriticalNotifyCompletion
    {
        public Action Continuation;

        public void UnsafeOnCompleted(Action continuation) => Continuation = continuation;

        public void OnCompleted(Action continuation) => Continuation = continuation;
    }

    //private class ContinuationWrapper(Action m_continuation)
    //{
    //    private TaskCompletionSource m_task = new();

    //    public void Run()
    //    {
    //        _ = m_task.Task.ContinueWith((task) => {
    //            m_continuation();
    //        }, TaskScheduler.Default);
    //    }

    //    public void Commit()
    //    {
    //        m_task.SetResult();
    //    }
    //}

    public class CoroutineSite
    {
        internal Channel<IStepCompletionHandler> IncomingStepChannel = Channel.CreateUnbounded<IStepCompletionHandler>();
    }
}

public class CoroutineTreeNode
{

}
