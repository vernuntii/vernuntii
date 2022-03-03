using System.Diagnostics;

namespace Vernuntii.Git.Diagnostics
{
    internal class SimpleProcessStartInfo
    {
        internal ProcessStartInfo ProcessStartInfo;

        /// <summary>
        /// Creates an instance of type <see cref="SimpleProcessStartInfo" />.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <param name="workingDirectory">The working directory is not used to find the executable. Instead, its value applies to the process that is started and only has meaning within the context of the new process.</param>
        public SimpleProcessStartInfo(string name, string? args = null, string? workingDirectory = null)
        {
            if (args == null) {
                ProcessStartInfo = new ProcessStartInfo(name) {
                    WorkingDirectory = workingDirectory
                };
            } else {
                ProcessStartInfo = new ProcessStartInfo(name, args) {
                    WorkingDirectory = workingDirectory
                };
            }

            ProcessStartInfo.UseShellExecute = false;
            ProcessStartInfo.RedirectStandardOutput = true;
            ProcessStartInfo.RedirectStandardError = true;
            ProcessStartInfo.CreateNoWindow = true;
        }
    }
}
