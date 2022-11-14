using System.Runtime.InteropServices;
using System.Text;

namespace Vernuntii.Git.LibGit2.Helpers
{
    /// <summary>
    /// The Utf8Converter service provides callers with static methods
    /// to convert from managed strings to and from UTF8 byte arrays.
    /// </summary>
    public static class Utf8Converter
    {
        // Try to be conservative in what we send and liberal in what we
        // accept; avoid sending invalid strings that can't be marshaled
        // to UTF, but always try to create a string for end-users from
        // native data.
        private static readonly Encoding toNativeEncoding = new UTF8Encoding(false, true);
        private static readonly Encoding fromNativeEncoding = new UTF8Encoding(false, false);

        /// <summary>
        /// Create a null-terminated UTF8 byte array representing the
        /// given managed string.
        /// </summary>
        public static unsafe IntPtr ToNative(string str)
        {
            if (str == null) {
                return IntPtr.Zero;
            }

            var length = toNativeEncoding.GetByteCount(str);
            var buffer = (byte*)Marshal.AllocHGlobal(length + 1).ToPointer();

            if (length > 0) {
                fixed (char* pValue = str) {
                    toNativeEncoding.GetBytes(pValue, str.Length, buffer, length);
                }
            }
            buffer[length] = 0;

            return new IntPtr(buffer);
        }

        /// <summary>
        /// Create managed string from the null-terminated UTF8 byte array
        /// representing a native string.
        /// </summary>
        public static unsafe string FromNative(byte* buf)
        {
            var end = buf;

            if (buf == null) {
                return null!;
            } else if (*buf == '\0') {
                return string.Empty;
            }

            while (*end != '\0') {
                end++;
            }

            return new string((sbyte*)buf, 0, (int)(end - buf), fromNativeEncoding);
        }
    }
}
