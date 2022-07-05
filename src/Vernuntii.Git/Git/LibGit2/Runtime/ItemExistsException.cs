using System.Runtime.Serialization;

namespace Vernuntii.Git.LibGit2.Runtime
{
    [Serializable]
    internal class ItemExistsException : Exception
    {
        public ItemExistsException()
        {
        }

        public ItemExistsException(string? message) : base(message)
        {
        }

        public ItemExistsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ItemExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}