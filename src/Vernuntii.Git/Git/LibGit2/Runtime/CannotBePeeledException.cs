using System.Runtime.Serialization;

namespace Vernuntii.Git.LibGit2.Runtime
{
    [Serializable]
    internal class CannotBePeeledException : Exception
    {
        public CannotBePeeledException()
        {
        }

        public CannotBePeeledException(string? message) : base(message)
        {
        }

        public CannotBePeeledException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected CannotBePeeledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}