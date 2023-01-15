using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Vernuntii.Console.MSBuild;

internal static class EncodingExtensions
{
    public static string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes, int length)
    {
        unsafe {
            fixed (byte* writtenbytes = &MemoryMarshal.GetReference(bytes)) {
                return encoding.GetString(writtenbytes, length);
            }
        }
    }

    public static string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes) =>
        encoding.GetString(bytes, bytes.Length);
}
