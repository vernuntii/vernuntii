namespace Vernuntii.Diagnostics
{
    internal class SimpleAsyncProcess : SimpleProcess, ISimpleAsyncProcess, IDisposable
    {
        public SimpleAsyncProcess(
          SimpleProcessStartInfo processStartInfo,
          bool echoCommand = false,
          string? commandEchoPrefix = null,
          Action<string?>? outputReceived = null,
          bool shouldStreamOutput = false,
          Action<string?>? errorReceived = null,
          bool shouldStreamError = false,
          bool shouldThrowOnNonZeroCode = false)
          : base(
                processStartInfo,
                echoCommand,
                commandEchoPrefix,
                outputReceived,
                shouldStreamOutput,
                errorReceived,
                shouldStreamError,
                shouldThrowOnNonZeroCode)
        {
        }

        public async Task<int> WaitForExitAsync()
        {
            EnsureProcessStarted();

            if (!ShouldStreamOutput) {
                string endAsync = await Process.StandardOutput.ReadToEndAsync();
                ReceiveOutput(endAsync);
            }

            if (!ShouldStreamError) {
                string endAsync = await Process.StandardError.ReadToEndAsync();
                ReceiveError(endAsync);
            }

            await Process.WaitForExitAsync();
            await Task.Run(new Action(Process.WaitForExit));
            ThrowOnNonZeroExitCode();
            return Process.ExitCode;
        }
    }
}
