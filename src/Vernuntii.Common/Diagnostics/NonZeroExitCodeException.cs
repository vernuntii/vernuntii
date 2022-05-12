namespace Vernuntii.Diagnostics
{
    internal class NonZeroExitCodeException : Exception
    {
        public int ExitCode { get; }

        public NonZeroExitCodeException()
        {
        }

        public NonZeroExitCodeException(string? message)
          : base(message)
        {
        }

        public NonZeroExitCodeException(string? message, Exception? innerException)
          : base(message, innerException)
        {
        }

        public NonZeroExitCodeException(int exitCode) => ExitCode = exitCode;

        public NonZeroExitCodeException(int exitCode, string? message)
          : base(message)
        {
            ExitCode = exitCode;
        }

        public NonZeroExitCodeException(int exitCode, string? message, Exception? innerException)
          : base(message, innerException)
        {
            ExitCode = exitCode;
        }
    }
}
