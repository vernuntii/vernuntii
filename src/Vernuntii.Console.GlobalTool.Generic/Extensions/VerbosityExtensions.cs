using Vernuntii.Logging;

namespace Vernuntii.Console.GlobalTool.Extensions
{
    internal static class VerbosityExtensions
    {
        public static string ToDotNetVerbosity(this Verbosity verbosity) => verbosity switch {
            Verbosity.Verbose => "normal",
            Verbosity.Debug => "normal",
            Verbosity.Information => "minimal",
            Verbosity.Warning => "minimal",
            Verbosity.Error => "minimal",
            Verbosity.Fatal => "quiet",
            _ => throw new ArgumentException($"Verbosity \"{verbosity}\" not known")
        };
    }
}
