using System.IO.Hashing;
using System.Text;

namespace Vernuntii.Console.MSBuild;

internal static class StringExtensions
{
    public static string ToCrc32HexString(this string value) =>
        HexMate.Convert.ToHexString(Crc32.Hash(Encoding.ASCII.GetBytes(value)));
}
