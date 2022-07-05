using System.Runtime.Serialization;

namespace Vernuntii.Git.LibGit2.Runtime
{
    [Serializable]
    internal class DoggedException : Exception
    {
        public DoggedException()
        {
        }

        public DoggedException(string? message) : base(message)
        {
        }

        public DoggedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DoggedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}