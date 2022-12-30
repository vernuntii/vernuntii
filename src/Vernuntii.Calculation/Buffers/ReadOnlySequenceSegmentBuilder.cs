using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Buffers
{
    internal class ReadOnlySequenceSegmentBuilder<T>
    {
        /// <summary>
        /// Total length of all <typeparamref name="T"/>'s.
        /// </summary>
        public int TotalLength { get; private set; }

        private Segment? _begin;
        private Segment? _end;

        public void AddMemory(ReadOnlyMemory<T> memory)
        {
            Segment.Add(ref _begin, ref _end, memory);
            TotalLength += memory.Length;
        }

        public void AddMemory(IEnumerable<ReadOnlyMemory<T>> memoryValues)
        {
            foreach (var memory in memoryValues) {
                AddMemory(memory);
            }
        }

        public void BuildSegments(out ReadOnlySequenceSegment<T> begin, out ReadOnlySequenceSegment<T> end)
        {
            if (_begin == null) {
                begin = Segment.EmptyBegin;
                end = Segment.EmptyEnd;
            } else {
                begin = _begin;

                if (_end == null) {
                    end = Segment.CreateEmptyEnd(_begin);
                } else {
                    end = _end;
                }
            }
        }

        public ReadOnlySequence<T> BuildSequence(int length, out IEnumerable<ReadOnlyMemory<T>>? rest)
        {
            BuildSegments(out var begin, out _);
            var end = begin;
            int endRunningLength;
            bool isEndInsideLength;

            nextEnd:
            endRunningLength = (int)end.RunningIndex + end.Memory.Length;
            isEndInsideLength = endRunningLength <= length;

            if (isEndInsideLength && end.Next != null) {
                end = end.Next;
                goto nextEnd;
            }

            // 'isEndInsideLength' is true if next of end is equals null
            // and total length of end is smaller than specified length.

            if (isEndInsideLength) {
                rest = null;
                return new ReadOnlySequence<T>(begin, 0, end, end.Memory.Length);
            } else {
                // How many bytes at the end of end-segment are rest?
                var endRestLength = endRunningLength - length;
                var endRestIndex = end.Memory.Length - endRestLength;

                var restCollection = new List<ReadOnlyMemory<T>>() {
                    end.Memory.Slice(endRestIndex)
                };

                CollectRest(restCollection, end.Next);
                rest = restCollection;
                return new ReadOnlySequence<T>(begin, 0, end, endRestIndex);
            }

            void CollectRest(ICollection<ReadOnlyMemory<T>> collection, ReadOnlySequenceSegment<T>? segment)
            {
                while (segment != null) {
                    collection.Add(segment.Memory);
                    segment = segment.Next;
                }
            }
        }

        private class Segment : ReadOnlySequenceSegment<T>
        {
            public static readonly Segment EmptyEnd = new Segment(ReadOnlyMemory<T>.Empty);
            public static readonly Segment EmptyBegin = new Segment(ReadOnlyMemory<T>.Empty) { Next = EmptyEnd };

            public static Segment CreateEmptyEnd(Segment begin) => new Segment(ReadOnlyMemory<T>.Empty) {
                Next = begin,
                RunningIndex = begin.RunningIndex,
            };

            public static void Add([AllowNull] ref Segment begin, ref Segment? end, ReadOnlyMemory<T> memory)
            {
                if (begin == null) {
                    begin = new Segment(memory);
                } else {
                    Segment previous;

                    if (end == null) {
                        previous = begin;
                    } else {
                        previous = end;
                    }

                    end = new Segment(memory) {
                        RunningIndex = previous.RunningIndex + previous.Memory.Length
                    };

                    previous.Next = end;
                }
            }

            private Segment(ReadOnlyMemory<T> memory) =>
                Memory = memory;
        }
    }
}
