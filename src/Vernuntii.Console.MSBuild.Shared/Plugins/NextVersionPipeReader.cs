using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace Vernuntii.Plugins;

internal class NextVersionPipeReader
{
    public static NextVersionPipeReader Create(Stream inboundNextVersion) =>
        new NextVersionPipeReader(PipeReader.Create(inboundNextVersion, new StreamPipeReaderOptions(leaveOpen: true)));

    private readonly PipeReader _inboundNextVersion;

    private NextVersionPipeReader(PipeReader inboundNextVersion) =>
        _inboundNextVersion = inboundNextVersion;

    public async Task<NextVersionDaemonProtocolMessageType> ReadNextVersionAsync(Stream outboundNextVersion)
    {
        byte messageType;
        var result = await _inboundNextVersion.ReadAsync();
        var advancingBuffer = result.Buffer;

        if (result.IsCompleted) {
            return NextVersionDaemonProtocolMessageType.Failure;
        }

        messageType = result.Buffer.First.Span[0];
        _inboundNextVersion.AdvanceTo(advancingBuffer.GetPosition(offset: 1));

        while (true) {
            result = await _inboundNextVersion.ReadAsync();

            if (result.IsCompleted) {
                break;
            }

            advancingBuffer = result.Buffer;
            var delimiterPosition = advancingBuffer.PositionOf(NextVersionDaemonProtocolDefaults.Delimiter);
            var copyingBuffer = advancingBuffer;

            if (delimiterPosition.HasValue) {
                copyingBuffer = advancingBuffer.Slice(0, delimiterPosition.Value);
            }

            var copyingBufferEnumerator = copyingBuffer.GetEnumerator();

            while (copyingBufferEnumerator.MoveNext()) {
                await outboundNextVersion.WriteAsync(copyingBufferEnumerator.Current);
            }

            _inboundNextVersion.AdvanceTo(consumed: advancingBuffer.End, examined: advancingBuffer.End);

            if (delimiterPosition.HasValue) {
                break;
            }
        }

        return (NextVersionDaemonProtocolMessageType)messageType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Exception CreateInvalidMessageTypeException(NextVersionDaemonProtocolMessageType resultType) =>
        new NextVersionApiException($"The {nameof(Vernuntii)} daemon process reported an invalid message type:" +
            $" the message type must be either represent {(int)NextVersionDaemonProtocolMessageType.Success} (success)" +
            $" or {(int)NextVersionDaemonProtocolMessageType.Failure} (failure) but got {resultType}");

    public async Task ValidateNextVersionAsync(NextVersionDaemonProtocolMessageType nextVersionMessageType, Stream nextVersionMessage)
    {
        if (nextVersionMessageType == NextVersionDaemonProtocolMessageType.Success) {
            return;
        }

        if (nextVersionMessageType == NextVersionDaemonProtocolMessageType.Failure) {
            var errorReader = new StreamReader(nextVersionMessage, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: true);
            var error = await errorReader.ReadToEndAsync();
            throw new NextVersionApiException($"The {nameof(Vernuntii)} daemon process produced an error: {error}");
        }

        throw CreateInvalidMessageTypeException(nextVersionMessageType);
    }

    /// <summary>
    /// Validates the next version output
    /// </summary>
    /// <param name="nextVersionMessageType"></param>
    /// <param name="nextVersionMessageProvider">Only evaluated in case of a bad message type. </param>
    /// <exception cref="NextVersionApiException"></exception>
    public void ValidateNextVersion(NextVersionDaemonProtocolMessageType nextVersionMessageType, Func<byte[]> nextVersionMessageProvider)
    {
        if (nextVersionMessageType == NextVersionDaemonProtocolMessageType.Success) {
            return;
        }

        if (nextVersionMessageType == NextVersionDaemonProtocolMessageType.Failure) {
            var error = Encoding.UTF8.GetString(nextVersionMessageProvider());
            throw new NextVersionApiException($"The {nameof(Vernuntii)} daemon process produced an error: {error}");
        }

        throw CreateInvalidMessageTypeException(nextVersionMessageType);
    }
}
