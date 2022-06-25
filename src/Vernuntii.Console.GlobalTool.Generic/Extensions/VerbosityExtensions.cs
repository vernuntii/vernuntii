using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nuke.Common.Tools.DotNet;
using Vernuntii.Logging;

namespace Vernuntii.Console.GlobalTool.Extensions
{
    internal static class VerbosityExtensions
    {
        public static string ToDotNetVerbosity(this Verbosity logLevel) => logLevel switch {
            Verbosity.Verbose => "normal",
            Verbosity.Debug => "normal",
            Verbosity.Information => "minimal",
            Verbosity.Warning => "minimal",
            Verbosity.Error => "minimal",
            Verbosity.Fatal => "quiet",
            _ => throw new ArgumentException($"Log level \"{logLevel}\" not convertible to type {typeof(DotNetVerbosity)}")
        };
    }
}
