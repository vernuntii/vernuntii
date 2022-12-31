namespace Vernuntii.PluginSystem;

/// <summary>
/// The implementer allows you to add a disposable for which he takes responsibility for disposing of at the end of its lifecycle.
/// </summary>
public interface IDisposableRegistrar
{
    /// <summary>
    /// Adds a disposable for which this instance takes responsibility for disposing of at the end of its lifecycle
    /// </summary>
    /// <param name="disposable"></param>
    void AddDisposable(IDisposable disposable);
}
