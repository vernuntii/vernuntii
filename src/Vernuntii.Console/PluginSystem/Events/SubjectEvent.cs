using System.Reactive.Subjects;

namespace Vernuntii.PluginSystem.Events
{
    /// <inheritdoc/>
    public class SubjectEvent : SubjectEvent<object?>
    {
        /// <summary>
        /// Subscribes an element handler to an observable sequence.
        /// </summary>
        public void Publish() =>
            Publish(null);

        /// <summary>
        /// Subscribes an element handler to an observable sequence.
        /// </summary>
        /// <param name="onNext"></param>
        public IDisposable Subscribe(Action onNext) =>
            this.Subscribe(_ => onNext());
    }

    /// <summary>
    /// An event for subscribing and publishing values of type <typeparamref name="TPayload"/>.
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public class SubjectEvent<TPayload> : IObserver<TPayload>, IObservable<TPayload>, IEventFactory
    {
        private Subject<TPayload> _subject { get; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public SubjectEvent() =>
            _subject = new Subject<TPayload>();

        /// <inheritdoc/>
        void IObserver<TPayload>.OnCompleted() => _subject.OnCompleted();

        /// <inheritdoc/>
        void IObserver<TPayload>.OnError(Exception error) => _subject.OnError(error);

        private void onNext(TPayload value) => _subject.OnNext(value);

        /// <summary>
        /// Notifies all subscribed observers about the arrival of the specified element in the sequence.
        /// </summary>
        /// <param name="value"></param>
        public void Publish(TPayload value) => onNext(value);

        /// <inheritdoc/>
        void IObserver<TPayload>.OnNext(TPayload value) => onNext(value);

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<TPayload> observer) => _subject.Subscribe(observer);

        /// <summary>
        /// Creates a new instance of this type.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected virtual object CreateEvent() =>
            Activator.CreateInstance(GetType()) ?? throw new InvalidOperationException(
                "Could not create an event of the type of this instance." +
                $" Consider providing a public constructor or override {nameof(CreateEvent)}.");

        object IEventFactory.CreateEvent() => CreateEvent();
    }
}
