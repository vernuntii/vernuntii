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

        public string? ReceivingPipeName {
            get => _receivingPipeName;

            set {
                CheckPipeHandle(value);
                _receivingPipeName = value;
                _hasCustomReceivingPipeHandle = true;
            }
        }

        [MemberNotNullWhen(true, nameof(ReceivingPipeName))]
        internal bool _hasCustomReceivingPipeHandle { get; set; }

        internal CancellationToken WaitForConnectionCancellatonToken { get; set; }

        private string? _receivingPipeName;

        internal Pipes(string? receivingPipeHandle) =>
            _receivingPipeName = receivingPipeHandle;

        [MemberNotNull(nameof(ReceivingPipeName))]
        internal void CheckPipes()
        {
            if (!_hasCustomReceivingPipeHandle) {
                CheckPipeHandle(ReceivingPipeName);
            }
        }
    }
}
