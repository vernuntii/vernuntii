namespace Vernuntii.Plugins;

public class CommandLineArgumentsException : ArgumentException
{
    public CommandLineArgumentsException() : base()
    {
    }

    public CommandLineArgumentsException(string? message) : base(message)
    {
    }

    public CommandLineArgumentsException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
