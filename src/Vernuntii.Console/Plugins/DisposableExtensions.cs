using Vernuntii.PluginSystem;

namespace Vernuntii.Plugins;

internal static class DisposableExtensions
{
    public static void DisposeWhenDisposing(this IDisposable disposable, Plugin plugin) =>
        plugin.AddDisposable(disposable);
}
