using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive;

internal class ZipEvent<T1, T2> : EveryEvent<(T1, T2)>
{
    [MemberNotNullWhen(true,
        nameof(_leftQueue),
        nameof(_rightQueue))]
    private bool _isOperable => _leftQueue != null && _rightQueue != null;

    private readonly IObservableEvent<T1> _left;
    private readonly IObservableEvent<T2> _right;
    private Queue<T1>? _leftQueue;
    private Queue<T2>? _rightQueue;
    private readonly LeftEventHandler _leftHandler;
    private readonly RightEventHandler _rightHandler;

    public ZipEvent(IObservableEvent<T1> left, IObservableEvent<T2> right)
    {
        _leftHandler = new(this);
        _rightHandler = new(this);
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
    }

    private void AttemptInitialize()
    {
        if (_isOperable) {
            return;
        }

        _leftQueue = new();
        _rightQueue = new();
    }

    private void AttemptDeinitialize()
    {
        if (!_isOperable || HasEventEntries) {
            return;
        }

        static void DeinitializeQueue<T>(ref Queue<T>? queue)
        {
            queue?.Clear();
            queue = null;
        }

        DeinitializeQueue(ref _leftQueue);
        DeinitializeQueue(ref _rightQueue);
    }

    public override IDisposable Subscribe(IEventObserver<(T1, T2)> eventHandler)
    {
        AttemptInitialize();

        return DelegatingDisposable.Create(new Action[] {
            base.Subscribe(eventHandler).Dispose,
            _left.Subscribe(_leftHandler).Dispose,
            _right.Subscribe(_rightHandler).Dispose,
            AttemptDeinitialize
        });
    }

    [MemberNotNull(
        nameof(_leftQueue),
        nameof(_rightQueue))]
    private void ThrowIfNotInitialized()
    {
        if (!_isOperable) {
            throw new InvalidOperationException("The zip event is not initialized");
        }
    }

    private void Evaluate<TFulfilled, TOther>(
        EventFulfillmentContext context,
        TFulfilled eventData,
        Queue<TFulfilled> queue,
        Queue<TOther> otherQueue,
        Func<TFulfilled, TOther, (T1, T2)> zipEventDataFactory)
    {
        if (otherQueue.Count == 0) {
            queue.Enqueue(eventData);
            return;
        }

        var otherEventData = otherQueue.Dequeue();
        var zipEventData = zipEventDataFactory(eventData, otherEventData);
        Evaluate(context, zipEventData);
    }

    public void OnFulfilled(EventFulfillmentContext context, T1 eventData)
    {
        ThrowIfNotInitialized();

        Evaluate(
            context,
            eventData,
            _leftQueue,
            _rightQueue,
            static (eventData, otherEventData) => (eventData, otherEventData));
    }

    public void OnFulfilled(EventFulfillmentContext context, T2 eventData)
    {
        ThrowIfNotInitialized();

        Evaluate(
            context,
            eventData,
            _rightQueue,
            _leftQueue,
            static (eventData, otherEventData) => (otherEventData, eventData));
    }

    private class LeftEventHandler : IUnschedulableEventObserver<T1>
    {
        private readonly ZipEvent<T1, T2> _condition;

        public LeftEventHandler(ZipEvent<T1, T2> condition) =>
            _condition = condition;

        public void OnFulfilled(EventFulfillmentContext context, T1 eventData) =>
            _condition.OnFulfilled(context, eventData);
    }

    private class RightEventHandler : IUnschedulableEventObserver<T2>
    {
        private readonly ZipEvent<T1, T2> _condition;

        public RightEventHandler(ZipEvent<T1, T2> condition) =>
            _condition = condition;

        public void OnFulfilled(EventFulfillmentContext context, T2 eventData) =>
            _condition.OnFulfilled(context, eventData);
    }
}
