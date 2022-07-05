using System.Runtime.Serialization;

namespace Vernuntii.Git.LibGit2.Runtime
{
    [Serializable]
    internal class InvalidSpecificationException : Exception
    {
        public InvalidSpecificationException()
        {
        }

        public InvalidSpecificationException(string? message) : base(message)
        {
        }

        public InvalidSpecificationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidSpecificationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}