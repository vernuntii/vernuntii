using System.Runtime.Serialization;

namespace Vernuntii.Git.LibGit2.Runtime
{
    [Serializable]
    internal class BareRepositoryException : Exception
    {
        public BareRepositoryException()
        {
        }

        public BareRepositoryException(string? message) : base(message)
        {
        }

        public BareRepositoryException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected BareRepositoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}