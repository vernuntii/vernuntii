using System.Buffers;
using FluentAssertions;
using Xunit;

namespace Vernuntii.Buffers;

public static class ByteArrayExtensions
{
    public static byte[][] Repeat(this byte[] data, int count) =>
        Enumerable.Repeat(data, count).ToArray();

    public static byte[][] Repeat(this byte[] data, long count) =>
        Enumerable.Repeat(data, (int)count).ToArray();
}

public class CeilingBlockReaderTests
{
    private static List<ReadOnlyMemory<byte>> ReadOnlyMemoryList(params byte[][] arrays) =>
        arrays.Select(x => (ReadOnlyMemory<byte>)x).ToList();

    private static List<byte[]> ArrayList(params byte[][] arrays) =>
        arrays.ToList();

    private static List<byte[]> ArrayList(params byte[][][] arrays) =>
        arrays.SelectMany(x => x).ToList();

    public static IEnumerable<object[]> NextBlocksShouldMatchGenerator()
    {
        yield return new object[] {
            1,
            ReadOnlyMemoryList(new byte[1]),
            ArrayList(new byte[1])
        };

        yield return new object[] {
            2,
            ReadOnlyMemoryList(new byte[2]),
            ArrayList(new byte[2])
        };

        yield return new object[] {
            1,
            ReadOnlyMemoryList(new byte[1].Repeat(2)),
            ArrayList(new byte[1].Repeat(2))
        };

        yield return new object[] {
            1,
            ReadOnlyMemoryList(new byte[2]),
            ArrayList(new byte[1].Repeat(2))
        };

        yield return new object[] {
            2,
            ReadOnlyMemoryList(new byte[1]),
            ArrayList(new byte[1])
        };

        yield return new object[] {
            2,
            ReadOnlyMemoryList(new byte[1].Repeat(3)),
            ArrayList(new byte[2],new byte[1])
        };
    }

    [Theory]
    [MemberData(nameof(NextBlocksShouldMatchGenerator))]
    public void Next_blocks_should_match(int blockSize, List<ReadOnlyMemory<byte>> byteArrays, List<byte[]> expectedBlocks)
    {
        var blockReader = new CeilingBlockReader<byte>(byteArrays.GetEnumerator(), blockSize);
        var blocks = new List<ReadOnlySequence<byte>>();

        checkNextBlock:
        var block = blockReader.NextBlock();

        if (block != null) {
            blocks.Add(block.Value);
            goto checkNextBlock;
        }

        blocks.Select(x => x.ToArray()).Should().BeEquivalentTo(expectedBlocks);
    }
}
