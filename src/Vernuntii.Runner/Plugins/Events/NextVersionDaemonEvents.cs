using System.Diagnostics.CodeAnalysis;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins.Events;

internal class NextVersionDaemonEvents
{
    public static readonly EventDiscriminator<PipeHandles> OnEditPipeHandles = EventDiscriminator.New<PipeHandles>();

    public sealed class PipeHandles
    {
        private static void CheckPipeHandle([NotNull] string? pipeHandle)
        {
            if (string.IsNullOrWhiteSpace(pipeHandle)) {
                throw new ArgumentException($"'{nameof(pipeHandle)}' cannot be null or whitespace.", nameof(pipeHandle));
            }
        }

        public string? ReceivingPipeHandle {
            get => _receivingPipeHandle;

            set {
                CheckPipeHandle(value);
                _receivingPipeHandle = value;
                _hasCustomReceivingPipeHandle = true;
            }
        }

        public string? SendingPipeHandle {
            get => _sendingPipeHandle;

            set {
                CheckPipeHandle(value);
                _sendingPipeHandle = value;
                _hasCustomSendingPipeHandle = true;
            }
        }

        [MemberNotNullWhen(true, nameof(ReceivingPipeHandle))]
        internal bool _hasCustomReceivingPipeHandle { get; set; }

        [MemberNotNullWhen(true, nameof(SendingPipeHandle))]
        internal bool _hasCustomSendingPipeHandle { get; set; }

        private string? _receivingPipeHandle;
        private string? _sendingPipeHandle;

        internal PipeHandles(string? receivingPipeHandle, string? sendingPipeHandle)
        {
            _receivingPipeHandle = receivingPipeHandle;
            _sendingPipeHandle = sendingPipeHandle;
        }

        [MemberNotNull(nameof(ReceivingPipeHandle), nameof(SendingPipeHandle))]
        internal void CheckPipeHandles()
        {
            if (!_hasCustomReceivingPipeHandle) {
                CheckPipeHandle(ReceivingPipeHandle);
            }

            if (!_hasCustomSendingPipeHandle) {
                CheckPipeHandle(SendingPipeHandle);
            }
        }
    }
}
