using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Teronis.Extensions;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Runner;

internal class VernuntiiDaemonizer : IVernuntiiRunner
{
    public static bool ShouldDaemonize(Span<string> consoleArguments, [NotNullWhen(true)] out string[]? trimmedConsoleArguments)
    {
        if (consoleArguments.Length != 0 && consoleArguments[0] == "--daemonize") {
            trimmedConsoleArguments = new string[consoleArguments.Length - 1];
            consoleArguments[1..].CopyTo(trimmedConsoleArguments);
            return true;
        }

        trimmedConsoleArguments = null;
        return false;
    }

    public string[] ConsoleArguments { get; }
    public IEventSystem PluginEvents => throw new NotSupportedException();
    public IPluginRegistry Plugins => throw new NotSupportedException();

    public VernuntiiDaemonizer(string[] consoleArguments) =>
        ConsoleArguments = consoleArguments;

    public Task<NextVersionResult> NextVersionAsync() => throw new NotSupportedException();

    public Task<int> RunAsync()
    {
        var assemblyPath = Assembly.GetCallingAssembly().Location ?? throw new InvalidOperationException();

        var executablePath = assemblyPath.TrimEnd(".dll", StringComparison.InvariantCultureIgnoreCase);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            executablePath += ".exe";
        }

        using var daemon = Process.Start(executablePath, ConsoleArguments);
        daemon.Close();
        return Task.FromResult(0);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
