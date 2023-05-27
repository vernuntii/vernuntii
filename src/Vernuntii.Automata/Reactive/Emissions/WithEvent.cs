using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive.Emissions;

/// <summary>
/// Every <typeparamref name="TSource"/> and <typeparamref name="TWith"/> event are getting queued.
/// If at least one of <typeparamref name="TSource"/> and <typeparamref name="TWith"/> are queued,
/// then they are together emitted and finally dequeued.
/// </summary>
/// <typeparam name="TSource"></typeparam>
/// <typeparam name="TWith"></typeparam>
internal class WithEvent<TSource, TWith> : Event<(TSource, TWith)>
{
    [MemberNotNullWhen(true,
        nameof(_sourceQueue),
        nameof(_withQueue))]
    private bool _isOperable => _sourceQueue != null && _withQueue != null;

    private readonly IObservableEvent<TSource> _source;
    private readonly IObservableEvent<TWith> _with;
    private Queue<TSource>? _sourceQueue;
    private Queue<TWith>? _withQueue;
    private readonly SourceEventHandler _sourceHandler;
    private readonly WithEventHandler _withHandler;

    public WithEvent(IObservableEvent<TSource> source, IObservableEvent<TWith> with)
    {
        _sourceHandler = new(this);
        _withHandler = new(this);
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _with = with ?? throw new ArgumentNullException(nameof(with));
    }

    private void AttemptInitialization()
    {
        if (_isOperable) {
            return;
        }

        _sourceQueue = new();
        _withQueue = new();
    }

    private void AttemptDeinitialization()
    {
        if (!_isOperable || HasEventEntries) {
            return;
        }

        static void DeinitializeQueue<T>(ref Queue<T>? queue)
        {
            queue?.Clear();
            queue = null;
        }

        DeinitializeQueue(ref _sourceQueue);
        DeinitializeQueue(ref _withQueue);
    }

    public override IDisposable Subscribe(IEventObserver<(TSource, TWith)> eventObserver)
    {
        AttemptInitialization();

        return DelegatingDisposable.Create(
            base.Subscribe(eventObserver).Dispose,
            _source.Subscribe(_sourceHandler).Dispose,
            _with.Subscribe(_withHandler).Dispose,
            AttemptDeinitialization
        );
    }

    [MemberNotNull(
        nameof(_sourceQueue),
        nameof(_withQueue))]
    private void ThrowIfNotInitialized()
    {
        if (!_isOperable) {
            throw new InvalidOperationException("The zip event is not initialized");
        }
    }

    private void EvaluateValue<TEmitted, TOther>(
        EventEmissionBacklog emissionBacklog,
        TEmitted eventData,
        Queue<TEmitted> queue,
        Queue<TOther> otherQueue,
        Func<TEmitted, TOther, (TSource, TWith)> zipEventDataFactory)
    {
        if (otherQueue.Count == 0) {
            queue.Enqueue(eventData);
            return;
        }

        var otherEventData = otherQueue.Dequeue();
        var zipEventData = zipEventDataFactory(eventData, otherEventData);
        EvaluateEmission(emissionBacklog, zipEventData);
    }

    private void EvaluateSource(EventEmissionBacklog emissionBacklog, TSource eventData)
    {
        ThrowIfNotInitialized();

        EvaluateValue(
            emissionBacklog,
            eventData,
            _sourceQueue,
            _withQueue,
            static (eventData, otherEventData) => (eventData, otherEventData));
    }

    public void EvaluateWith(EventEmissionBacklog emissionBacklog, TWith eventData)
    {
        ThrowIfNotInitialized();

        EvaluateValue(
            emissionBacklog,
            eventData,
            _withQueue,
            _sourceQueue,
            static (eventData, otherEventData) => (otherEventData, eventData));
    }

    private class SourceEventHandler : IUnbackloggableEventObserver<TSource>
    {
        private readonly WithEvent<TSource, TWith> _condition;

        public SourceEventHandler(WithEvent<TSource, TWith> condition) =>
            _condition = condition;

        public void OnEmission(EventEmissionBacklog emissionBacklog, TSource eventData) =>
            _condition.EvaluateSource(emissionBacklog, eventData);
    }

    private class WithEventHandler : IUnbackloggableEventObserver<TWith>
    {
        private readonly WithEvent<TSource, TWith> _condition;

        public WithEventHandler(WithEvent<TSource, TWith> condition) =>
            _condition = condition;

        public void OnEmission(EventEmissionBacklog emissionBacklog, TWith eventData) =>
            _condition.EvaluateWith(emissionBacklog, eventData);
    }
}
