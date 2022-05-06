using System.Runtime.Serialization;

namespace Vernuntii.Git
{
    /// <summary>
    /// Represents the exception that occures when a repository is shallow.
    /// </summary>
    [Serializable]
    public class ShallowRepositoryException : Exception
    {
        /// <inheritdoc/>
        public override string Message {
            get {
                var baseMessage = base.Message;

                if (!string.IsNullOrEmpty(baseMessage) && !string.IsNullOrEmpty(GitDirectory)) {
                    return baseMessage + Environment.NewLine + $"Git directory: {GitDirectory}";
                }

                return Message;
            }
        }

        /// <summary>
        /// The path to the git directory.
        /// </summary>
        public string? GitDirectory { get; init; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public ShallowRepositoryException()
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public ShallowRepositoryException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public ShallowRepositoryException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        protected ShallowRepositoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
