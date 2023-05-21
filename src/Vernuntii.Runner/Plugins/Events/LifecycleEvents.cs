using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events not for a plugin but for the lifecycle.
    /// </summary>
    public static class LifecycleEvents
    {
        /// <summary>
        /// The event is called before a run is started.
        /// </summary>
        public static readonly EventDiscriminator<LifecycleContext> BeforeEveryRun = EventDiscriminator.New<LifecycleContext>();

        /// <summary>
        /// The event is called before next run starts. "Next run" refers to second and subsequent runs.
        /// </summary>
        public static readonly EventDiscriminator<LifecycleContext> BeforeNextRun = EventDiscriminator.New<LifecycleContext>();

        /// <summary>
        /// The event is called when the run ends.
        /// </summary>
        public static readonly EventDiscriminator EndOfRun = EventDiscriminator.New();

        /// <summary>
        /// Represents the context of a lifecycle.
        /// </summary>
        public sealed class LifecycleContext
        {
            private List<Task>? _backgroundTasks;
            private object _backgroundTasksLock = new();
            private bool _isAcceptingFurtherBackgroundTasks = true;

            private TaskCompletionSource RegisterBackgroundTaskCompletionSource()
            {
                TaskCompletionSource backgroundTaskSource;

                lock (_backgroundTasksLock) {
                    if (!_isAcceptingFurtherBackgroundTasks) {
                        throw new InvalidOperationException();
                    }

                    backgroundTaskSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously | TaskCreationOptions.LongRunning);

                    if (_backgroundTasks is null) {
                        _backgroundTasks = new List<Task>();
                    }

                    _backgroundTasks.Add(backgroundTaskSource.Task);
                }

                return backgroundTaskSource;
            }

            private void RegisterBackgroundTask(Task task)
            {
                lock (_backgroundTasksLock) {
                    if (!_isAcceptingFurtherBackgroundTasks) {
                        throw new InvalidOperationException();
                    }

                    if (_backgroundTasks is null) {
                        _backgroundTasks = new List<Task>();
                    }

                    _backgroundTasks.Add(task);
                }
            }

            public void NewBackgroundTask(Action<TaskCompletionSource> doBackgroundTask, CancellationToken cancellationToken)
            {
                if (doBackgroundTask is null) {
                    throw new ArgumentNullException(nameof(doBackgroundTask));
                }

                var taskCompletionSource = RegisterBackgroundTaskCompletionSource();
                _ = Task.Run(() => doBackgroundTask(taskCompletionSource), cancellationToken);
            }

            public void NewBackgroundTask(Action<TaskCompletionSource> doBackgroundTask) =>
                NewBackgroundTask(doBackgroundTask, CancellationToken.None);

            public void NewBackgroundTask(Func<Task> doBackgroundTask, CancellationToken cancellationToken)
            {
                if (doBackgroundTask is null) {
                    throw new ArgumentNullException(nameof(doBackgroundTask));
                }

                var backgroundTask = Task.Run(async () => await doBackgroundTask().ConfigureAwait(false), cancellationToken);
                RegisterBackgroundTask(backgroundTask);
            }

            public void NewBackgroundTask(Func<Task> doBackgroundTask) =>
                NewBackgroundTask(doBackgroundTask, CancellationToken.None);

            internal Task WaitForBackgroundTasks()
            {
                lock (_backgroundTasksLock) {
                    if (_backgroundTasks is null) {
                        return Task.CompletedTask;
                    }

                    _isAcceptingFurtherBackgroundTasks = false;
                    return Task.WhenAll(_backgroundTasks);
                }
            }
        }
    }
}
