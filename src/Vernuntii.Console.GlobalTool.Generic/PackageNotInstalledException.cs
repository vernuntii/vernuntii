using System.Runtime.Serialization;

namespace Vernuntii.Console.GlobalTool
{
    [Serializable]
    internal class PackageNotInstalledException : Exception
    {
        public PackageNotInstalledException()
        {
        }

        public PackageNotInstalledException(string? message) : base(message)
        {
        }

        public PackageNotInstalledException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected PackageNotInstalledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
