namespace Vernuntii.PluginSystem.Events
{
    internal class OneSignalSubscription : IOneSignalSubscription
    {
        public bool SignaledOnce {
            get => _signaledOnce;

            internal set {
                if (IsDisposed) {
                    throw new InvalidOperationException("You cannot signal once when the subscription has been disposed");
                }

                _signaledOnce = value;
                Dispose();
            }
        }

        public bool IsDisposed => Disposable is null;

        internal IDisposable? Disposable {
            get => _disposable;

            set {
                if (SignaledOnce) {
                    throw new InvalidOperationException("You cannot set disposable when the subscription has been signaled once");
                }

                _disposable = value;
            }
        }

        private bool _signaledOnce;
        private IDisposable? _disposable;

        public void Dispose()
        {
            if (_disposable is null) {
                return;
            }

            _disposable.Dispose();
            _disposable = null;
        }
    }
}
