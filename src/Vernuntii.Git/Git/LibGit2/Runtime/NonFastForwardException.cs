using System.Runtime.Serialization;

namespace Vernuntii.Git.LibGit2.Runtime
{
    [Serializable]
    internal class NonFastForwardException : Exception
    {
        public NonFastForwardException()
        {
        }

        public NonFastForwardException(string? message) : base(message)
        {
        }

        public NonFastForwardException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NonFastForwardException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}