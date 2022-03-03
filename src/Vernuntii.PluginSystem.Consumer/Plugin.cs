namespace Vernuntii.PluginSystem.Consumer
{
    /// <summary>
    /// A plugin for <see cref="Vernuntii"/>.
    /// </summary>
    public abstract class Plugin : IPlugin
    {
        /// <inheritdoc/>
        public int? Order { get; }

        /// <inheritdoc/>
        public void OnPluginRegistered(IPlugin plugin) { }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">True disposes managed state (managed objects).</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        protected virtual ValueTask DisposeAsyncCore() =>
            ValueTask.CompletedTask;

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }
    }
}
