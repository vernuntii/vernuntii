using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Vernuntii.Console.MSBuild;

public static class TaskExtensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>")]
    internal static async Task<T> WaitAsync<T>(this Task<T> task, TimeSpan timeSpan)
    {
        using var cancellationTokenSource = new CancellationTokenSource(timeSpan);
        return await task.WaitAsync(cancellationTokenSource.Token).ConfigureAwait(false);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>")]
    internal static async Task WaitAsync(this Task task, TimeSpan timeSpan)
    {
        using var cancellationTokenSource = new CancellationTokenSource(timeSpan);
        await task.WaitAsync(cancellationTokenSource.Token).ConfigureAwait(false);
    }
}
