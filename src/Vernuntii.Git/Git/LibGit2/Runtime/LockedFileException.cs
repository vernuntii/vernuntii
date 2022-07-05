using System.Runtime.Serialization;

namespace Vernuntii.Git.LibGit2.Runtime
{
    [Serializable]
    internal class LockedFileException : Exception
    {
        public LockedFileException()
        {
        }

        public LockedFileException(string? message) : base(message)
        {
        }

        public LockedFileException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected LockedFileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}