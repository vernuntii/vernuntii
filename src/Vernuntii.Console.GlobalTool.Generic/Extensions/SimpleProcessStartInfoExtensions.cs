using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vernuntii.Diagnostics;

namespace Vernuntii.Console.GlobalTool.Extensions
{
    internal static class SimpleProcessStartInfoExtensions
    {
        public static SimpleProcessStartInfo LogDebug(this SimpleProcessStartInfo startInfo, ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug)) {
                string message;

                if (startInfo.Arguments == null) {
                    message = "{DotNetName}";
                } else {
                    message = "{DotNetName} {DotNetArguments}";
                }

#pragma warning disable CA2254 // Template should be a static expression
                logger.LogDebug(message, startInfo.Executable, startInfo.Arguments);
#pragma warning restore CA2254 // Template should be a static expression
            }

            return startInfo;
        }
    }
}
