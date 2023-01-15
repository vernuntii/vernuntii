using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Vernuntii.Text;

internal static class EncodingExtensions
{
    public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes, int length)
    {
        fixed (byte* writtenbytes = &MemoryMarshal.GetReference(bytes)) {
            return encoding.GetString(writtenbytes, length);
        }
    }
}
