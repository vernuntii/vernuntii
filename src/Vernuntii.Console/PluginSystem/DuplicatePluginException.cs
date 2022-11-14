namespace Vernuntii.PluginSystem
{
    internal class DuplicatePluginException : Exception
    {
        public DuplicatePluginException() : base()
        {
        }

        public DuplicatePluginException(string? message) : base(message)
        {
        }

        public DuplicatePluginException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
