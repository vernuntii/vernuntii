using System.Runtime.ExceptionServices;

namespace Vernuntii.Coroutines;

internal class GlobalScope
{
    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    /// <summary>Throws the exception on the ThreadPool.</summary>
    /// <param name="exception">The exception to propagate.</param>
    /// <param name="targetContext">The target context on which to propagate the exception.  Null to use the ThreadPool.</param>
    [SuppressMessage("Roslynator", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "<pending>")]
    internal static void ThrowAsync(Exception exception, SynchronizationContext? targetContext)
    {
        // Capture the exception into an ExceptionDispatchInfo so that its
        // stack trace and Watson bucket info will be preserved
        var capturedError = ExceptionDispatchInfo.Capture(exception);

        // If the user supplied a SynchronizationContext...
        if (targetContext != null) {
            try {
                // Post the throwing of the exception to that context, and return.
                targetContext.Post(static state => ((ExceptionDispatchInfo)state!).Throw(), capturedError);
                return;
            } catch (Exception postException) {
                // If something goes horribly wrong in the Post, we'll
                // propagate both exceptions on the ThreadPool
                capturedError = ExceptionDispatchInfo.Capture(new AggregateException(exception, postException));
            }
        }

#if NATIVEAOT
        RuntimeExceptionHelpers.ReportUnhandledException(capturedError.SourceException);
#else
        // Propagate the exception(s) on the ThreadPool
        ThreadPool.QueueUserWorkItem(static state => ((ExceptionDispatchInfo)state!).Throw(), capturedError);
#endif // NATIVEAOT
    }
}
