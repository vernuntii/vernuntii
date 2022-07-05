using System.Runtime.Serialization;

namespace Vernuntii.Git.LibGit2.Runtime
{
    [Serializable]
    internal class UserCancelledException : Exception
    {
        public UserCancelledException()
        {
        }

        public UserCancelledException(string? message) : base(message)
        {
        }

        public UserCancelledException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected UserCancelledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}