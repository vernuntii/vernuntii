using Vernuntii.Plugins;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Runner;

/// <summary>
/// Represents the main entry point to calculate the next version.
/// </summary>
public interface IVernuntiiRunner
{
    /// <summary>
    /// The console arguments.
    /// </summary>
    string[] ConsoleArguments { get; }

    /// <summary>
    /// The plugin event system.
    /// </summary>
    IEventSystem PluginEvents { get; }

    /// <summary>
    /// Runs Vernuntii for getting the next version. The presence of <see cref="INextVersionPlugin"/> and its dependencies is expected.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    Task<NextVersionResult> NextVersionAsync();

    /// <summary>
    /// Runs Vernuntii for console.
    /// </summary>
    /// <returns>
    /// The promise of an exit code.
    /// </returns>
    Task<int> RunAsync();
}
