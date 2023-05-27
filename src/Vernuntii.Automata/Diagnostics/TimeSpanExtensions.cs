using System.Globalization;

namespace Vernuntii.Diagnostics;

// TODO: Move to snippets
internal static class TimeSpanExtensions
{
    public static string ToSecondsString(this TimeSpan elapsedTime) =>
        elapsedTime.ToString("s\\.fff", CultureInfo.InvariantCulture) + "s";
}
