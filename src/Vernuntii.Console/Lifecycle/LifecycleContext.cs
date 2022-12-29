namespace Vernuntii.Lifecycle;

/// <summary>
/// Represents the context of a lifecycle.
/// </summary>
public sealed class LifecycleContext
{
    /// <summary>
    /// The exit code the Vernuntii runner will use.
    /// </summary>
    public int? ExitCode { get; set; }
}
