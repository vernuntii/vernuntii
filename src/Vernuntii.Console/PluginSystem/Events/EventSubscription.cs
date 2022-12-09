namespace Vernuntii.PluginSystem.Events
{
    internal class EventSubscription : IEventSubscription
    {
        public int SignalCount =>
            _signalCount;

        public bool IsDisposed =>
            _disposable is null;

        internal IDisposable? Disposable {
            set => _disposable = value;
        }

        internal void IncrementCounter()
        {
            if (IsDisposed) {
                throw new InvalidOperationException("You cannot increment signal counter when the subscription has been already disposed");
            }

            Interlocked.Increment(ref _signalCount);
        }

        private int _signalCount;
        private IDisposable? _disposable;

        public void Dispose()
        {
            var disposable = _disposable;

            if (disposable is null) {
                return;
            }

            try {
                Interlocked.CompareExchange(ref _disposable, null, disposable)?.Dispose();
            } catch {
                // Underlying dispose implementation should never throw
            }
        }
    }
}
