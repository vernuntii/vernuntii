using System.Runtime.Serialization;

namespace Vernuntii.Console.GlobalTool
{
    [Serializable]
    internal class VersionNotSpecifiedException : Exception
    {
        public VersionNotSpecifiedException()
        {
        }

        public VersionNotSpecifiedException(string? message) : base(message)
        {
        }

        public VersionNotSpecifiedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected VersionNotSpecifiedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}