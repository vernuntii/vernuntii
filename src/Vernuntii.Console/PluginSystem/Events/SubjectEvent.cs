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
}
