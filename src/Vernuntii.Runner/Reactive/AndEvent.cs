using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive;

/// <summary>
/// Every <typeparamref name="TOperandA"/> and <typeparamref name="TOperandB"/> event are getting queued.
/// If at least one of <typeparamref name="TOperandA"/> and <typeparamref name="TOperandB"/> are queued,
/// then they are together emitted and finally dequeued.
/// </summary>
/// <typeparam name="TOperandA"></typeparam>
/// <typeparam name="TOperandB"></typeparam>
internal class AndEvent<TOperandA, TOperandB> : EveryEvent<(TOperandA, TOperandB)>
{
    [MemberNotNullWhen(true,
        nameof(_operandAQueue),
        nameof(_operandBQueue))]
    private bool _isOperable => _operandAQueue != null && _operandBQueue != null;

    private readonly IEmittableEvent<TOperandA> _operandA;
    private readonly IEmittableEvent<TOperandB> _operandB;
    private Queue<TOperandA>? _operandAQueue;
    private Queue<TOperandB>? _operandBQueue;
    private readonly OperandAEventHandler _operandAHandler;
    private readonly OperandBEventHandler _operandBHandler;

    public AndEvent(IEmittableEvent<TOperandA> operandA, IEmittableEvent<TOperandB> operandB)
    {
        _operandAHandler = new(this);
        _operandBHandler = new(this);
        _operandA = operandA ?? throw new ArgumentNullException(nameof(operandA));
        _operandB = operandB ?? throw new ArgumentNullException(nameof(operandB));
    }

    private void AttemptInitialization()
    {
        if (_isOperable) {
            return;
        }

        _operandAQueue = new();
        _operandBQueue = new();
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

        DeinitializeQueue(ref _operandAQueue);
        DeinitializeQueue(ref _operandBQueue);
    }

    public override IDisposable Subscribe(IEventEmitter<(TOperandA, TOperandB)> eventHandler)
    {
        AttemptInitialization();

        return DelegatingDisposable.Create(
            base.Subscribe(eventHandler).Dispose,
            _operandA.Subscribe(_operandAHandler).Dispose,
            _operandB.Subscribe(_operandBHandler).Dispose,
            AttemptDeinitialization
        );
    }

    [MemberNotNull(
        nameof(_operandAQueue),
        nameof(_operandBQueue))]
    private void ThrowIfNotInitialized()
    {
        if (!_isOperable) {
            throw new InvalidOperationException("The zip event is not initialized");
        }
    }

    private void EvaluateAnyOperand<TEmitted, TOther>(
        EventEmissionContext context,
        TEmitted eventData,
        Queue<TEmitted> queue,
        Queue<TOther> otherQueue,
        Func<TEmitted, TOther, (TOperandA, TOperandB)> zipEventDataFactory)
    {
        if (otherQueue.Count == 0) {
            queue.Enqueue(eventData);
            return;
        }

        var otherEventData = otherQueue.Dequeue();
        var zipEventData = zipEventDataFactory(eventData, otherEventData);
        EvaluateEmission(context, zipEventData);
    }

    private void EvaluateOperandA(EventEmissionContext context, TOperandA eventData)
    {
        ThrowIfNotInitialized();

        EvaluateAnyOperand(
            context,
            eventData,
            _operandAQueue,
            _operandBQueue,
            static (eventData, otherEventData) => (eventData, otherEventData));
    }

    public void EvaluateOperandB(EventEmissionContext context, TOperandB eventData)
    {
        ThrowIfNotInitialized();

        EvaluateAnyOperand(
            context,
            eventData,
            _operandBQueue,
            _operandAQueue,
            static (eventData, otherEventData) => (otherEventData, eventData));
    }

    private class OperandAEventHandler : IUnschedulableEventEmitter<TOperandA>
    {
        private readonly AndEvent<TOperandA, TOperandB> _condition;

        public OperandAEventHandler(AndEvent<TOperandA, TOperandB> condition) =>
            _condition = condition;

        public void Emit(EventEmissionContext context, TOperandA eventData) =>
            _condition.EvaluateOperandA(context, eventData);
    }

    private class OperandBEventHandler : IUnschedulableEventEmitter<TOperandB>
    {
        private readonly AndEvent<TOperandA, TOperandB> _condition;

        public OperandBEventHandler(AndEvent<TOperandA, TOperandB> condition) =>
            _condition = condition;

        public void Emit(EventEmissionContext context, TOperandB eventData) =>
            _condition.EvaluateOperandB(context, eventData);
    }
}
