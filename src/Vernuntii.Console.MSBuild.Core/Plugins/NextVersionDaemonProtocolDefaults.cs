namespace Vernuntii.Plugins;

internal class NextVersionDaemonProtocolDefaults
{
    public const string ServerName = ".";
    public const string MutexPrefix = "Global\\";
    public const byte Delimiter = 31; // Unit Separator
    public const byte Success = 0;
    public const byte Failure = 1;
}
