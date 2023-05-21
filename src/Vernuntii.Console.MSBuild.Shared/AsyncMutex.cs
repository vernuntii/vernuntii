using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vernuntii;

internal sealed class AsyncMutex : IAsyncDisposable
{
    private readonly string _name;
    private Task? _mutexContainingTask;
    private ManualResetEventSlim? _mutexReleaseEvent;
    private CancellationTokenSource? _cancellationTokenSource;

    public AsyncMutex(string name) =>
        _name = name;

    public Task AcquireAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TaskCompletionSource<object?> taskCompletionSource = new();

        _mutexReleaseEvent = new ManualResetEventSlim();
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Putting all mutex manipulation in its own task as it doesn't work in async contexts
        // Note: this task should not throw.
        _mutexContainingTask = Task.Factory.StartNew(
            _ => {
                try {
                    var cancellationToken = _cancellationTokenSource.Token;
                    using var mutex = new Mutex(initiallyOwned: false, _name);

                    try {
                        // Wait until either the mutex is acquired or the cancellation occurs
                        if (WaitHandle.WaitAny(new[] { mutex, cancellationToken.WaitHandle }) != 0) {
                            taskCompletionSource.TrySetCanceled(cancellationToken);
                            return;
                        }
                    } catch (AbandonedMutexException) {
                        // Abandoned by another process, but since we acquired it, we can ignore it.
                    }

                    // Wait until the mutex is about to be released
                    taskCompletionSource.SetResult(null);
                    _mutexReleaseEvent.Wait();
                    mutex.ReleaseMutex();
                } catch (OperationCanceledException) {
                    taskCompletionSource.TrySetCanceled(cancellationToken);
                } catch (Exception ex) {
                    taskCompletionSource.TrySetException(ex);
                }
            },
            state: null,
            cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

        return taskCompletionSource.Task;
    }

    public Task AcquireAsync() =>
        AcquireAsync(CancellationToken.None);

    public async Task AcquireAsync(TimeSpan timeout)
    {
        using var cancellationTokenSource = new CancellationTokenSource(timeout);
        await AcquireAsync(cancellationTokenSource.Token).ConfigureAwait(false);
    }

    public async Task AcquireAsync(int timeout)
    {
        using var cancellationTokenSource = new CancellationTokenSource(timeout);
        await AcquireAsync(cancellationTokenSource.Token).ConfigureAwait(false);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>")]
    public async Task ReleaseAsync()
    {
        _mutexReleaseEvent?.Set();

        if (_mutexContainingTask != null) {
            await _mutexContainingTask.ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        // Ensure the mutex task stops waiting for any acquisition
        _cancellationTokenSource?.Cancel();

        // Ensure the mutex is getting released
        await ReleaseAsync().ConfigureAwait(false);

        _mutexReleaseEvent?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}

