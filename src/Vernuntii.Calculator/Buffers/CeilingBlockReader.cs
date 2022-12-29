using System.Buffers;

namespace Vernuntii.Buffers
{
    internal class CeilingBlockReader<T>
    {
        public int MaxSizePerBlock { get; }

        private readonly IEnumerator<ReadOnlyMemory<T>> _byteArrays;
        private IEnumerable<ReadOnlyMemory<T>>? _rest;

        public CeilingBlockReader(IEnumerator<ReadOnlyMemory<T>> byteArrays, int maxSizePerBlock)
        {
            _byteArrays = byteArrays ?? throw new ArgumentNullException(nameof(byteArrays));
            MaxSizePerBlock = maxSizePerBlock;
        }

        public ReadOnlySequence<T>? NextBlock()
        {
            if (_byteArrays.MoveNext()) {
                var segmentBuilder = new ReadOnlySequenceSegmentBuilder<T>();

                if (_rest != null) {
                    segmentBuilder.AddMemory(_rest);
                }

                do {
                    var transactionBytes = _byteArrays.Current;
                    segmentBuilder.AddMemory(transactionBytes);
                } while (segmentBuilder.TotalLength < MaxSizePerBlock && _byteArrays.MoveNext());

                return segmentBuilder.BuildSequence(MaxSizePerBlock, out _rest);
            } else if (_rest != null) {
                var segmentBuilder = new ReadOnlySequenceSegmentBuilder<T>();
                segmentBuilder.AddMemory(_rest);
                return segmentBuilder.BuildSequence(MaxSizePerBlock, out _rest);
            }

            return null;
        }
    }
}
