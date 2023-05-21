using System.Diagnostics.CodeAnalysis;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins.Events;

internal class NextVersionDaemonEvents
{
    public static readonly EventDiscriminator<Pipes> OnEditPipes = EventDiscriminator.New<Pipes>();

    public sealed class Pipes
    {
        internal static void CheckPipeHandle([NotNull] string? pipeHandleName)
        {
            if (string.IsNullOrWhiteSpace(pipeHandleName)) {
                throw new ArgumentException($"Pipe name cannot be null or whitespace", nameof(pipeHandleName));
            }
        }

        public string? DaemonPipeServerName {
            get => _daemonPipeServerName;

            set {
                CheckPipeHandle(value);
                _daemonPipeServerName = value;
                _hasCustomIncomingPipeServerHandle = true;
            }
        }

        [MemberNotNullWhen(true, nameof(DaemonPipeServerName))]
        internal bool _hasCustomIncomingPipeServerHandle { get; set; }

        internal CancellationToken WaitForConnectionCancellatonToken { get; set; }

        private string? _daemonPipeServerName;

        internal Pipes(string? daemonPipeServerName) =>
            _daemonPipeServerName = daemonPipeServerName;

        [MemberNotNull(nameof(DaemonPipeServerName))]
        internal void CheckPipes()
        {
            if (!_hasCustomIncomingPipeServerHandle) {
                CheckPipeHandle(DaemonPipeServerName);
            }
        }
    }
}
