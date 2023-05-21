using System.Runtime.CompilerServices;

namespace Vernuntii.Plugins;

internal class NextVersionDaemonProtocolDefaults
{
    public const string ServerName = ".";
    public const string MutexPrefix = @"Global\";
    public const byte Delimiter = 31; // Unit Separator
    public const byte Success = 0;
    public const byte Failure = 1;

    /// <summary>
    /// A method for easier tracing who relies on the same name.
    /// </summary>
    /// <param name="daemonPipeServerName"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetDaemonServerMutexName(string daemonPipeServerName) =>
        NextVersionDaemonProtocolDefaults.MutexPrefix + daemonPipeServerName;
}
