using System.Runtime.Serialization;

namespace Vernuntii.Git.LibGit2.Runtime
{
    [Serializable]
    internal class CheckoutConflictException : Exception
    {
        public CheckoutConflictException()
        {
        }

        public CheckoutConflictException(string? message) : base(message)
        {
        }

        public CheckoutConflictException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected CheckoutConflictException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}