using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Vernuntii.Diagnostics
{
    internal static class ProcessStartInfoUtils
    {
        public static string? GetExecutionInfoText(ProcessStartInfo startInfo, string? commandEchoPrefix = null)
        {
            if (startInfo == null) {
                return null;
            }

            var echoBuilder = new StringBuilder();

            void appendCommandEchoPrefix()
            {
                if (commandEchoPrefix != null) {
                    echoBuilder.Append($"{commandEchoPrefix}: ");
                }
            }

            if (!string.IsNullOrEmpty(startInfo.WorkingDirectory)) {
                appendCommandEchoPrefix();
                echoBuilder.Append($"Working directory: {startInfo.WorkingDirectory}{Environment.NewLine}");
            }

            appendCommandEchoPrefix();
            echoBuilder.Append($"{startInfo.FileName} {startInfo.Arguments}");
            return echoBuilder.ToString();
        }
    }
}
