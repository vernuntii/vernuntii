using System.Runtime.Serialization;

namespace Vernuntii.Git.Diagnostics
{
    [Serializable]
    internal class ProcessNotSpawnedException : Exception
    {
        public ProcessNotSpawnedException()
        {
        }

        public ProcessNotSpawnedException(string message)
          : base(message)
        {
        }

        public ProcessNotSpawnedException(string message, Exception innerException)
          : base(message, innerException)
        {
        }

        protected ProcessNotSpawnedException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}
