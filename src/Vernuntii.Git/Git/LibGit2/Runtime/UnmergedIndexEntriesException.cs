using System.Runtime.Serialization;

namespace Vernuntii.Git.LibGit2.Runtime
{
    [Serializable]
    internal class UnmergedIndexEntriesException : Exception
    {
        public UnmergedIndexEntriesException()
        {
        }

        public UnmergedIndexEntriesException(string? message) : base(message)
        {
        }

        public UnmergedIndexEntriesException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected UnmergedIndexEntriesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}