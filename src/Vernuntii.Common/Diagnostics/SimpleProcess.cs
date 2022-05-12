using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Teronis.Extensions;

namespace Vernuntii.Diagnostics
{
    /// <summary>
    /// Represents the aquivalent of <see cref="Process"/>
    /// </summary>
    internal class SimpleProcess : IDisposable, ISimpleProcess
    {
        #region factory methods

        public static int StartThenWaitForExit(
          SimpleProcessStartInfo startInfo,
          bool echoCommand = false,
          string? commandEchoPrefix = null,
          Action<string?>? outputReceived = null,
          bool shouldStreamOutput = false,
          Action<string?>? errorReceived = null,
          bool shouldStreamError = false,
          bool shouldThrowOnNonZeroCode = false)
        {
            using SimpleProcess simpleProcess = new SimpleProcess(
                startInfo,
                echoCommand,
                commandEchoPrefix,
                outputReceived,
                shouldStreamOutput,
                errorReceived,
                shouldStreamError,
                shouldThrowOnNonZeroCode);

            simpleProcess.Start();
            return simpleProcess.WaitForExit();
        }

        public static async Task<int> StartThenWaitForExitAsync(
          SimpleProcessStartInfo startInfo,
          bool echoCommand = false,
          string? commandEchoPrefix = null,
          Action<string?>? outputReceived = null,
          bool shouldStreamOutput = false,
          Action<string?>? errorReceived = null,
          bool shouldStreamError = false,
          bool shouldThrowOnNonZeroCode = false)
        {
            using var process = new SimpleAsyncProcess(
                startInfo,
                echoCommand,
                commandEchoPrefix,
                outputReceived,
                shouldStreamOutput,
                errorReceived,
                shouldStreamError,
                shouldThrowOnNonZeroCode);

            process.Start();
            return await process.WaitForExitAsync();
        }

        public static string StartThenWaitForExitThenReadOutput(
          SimpleProcessStartInfo startInfo,
          bool echoCommand = false,
          string? commandEchoPrefix = null,
          Action<string?>? errorReceived = null,
          bool shouldStreamError = false,
          bool shouldThrowOnNonZeroCode = false)
        {
            StringBuilder outputBuilder = new StringBuilder();

            using SimpleProcess simpleProcess = new SimpleProcess(
                startInfo,
                echoCommand,
                commandEchoPrefix,
                output => outputBuilder.Append(output),
                errorReceived: errorReceived,
                shouldStreamError: shouldStreamError,
                shouldThrowOnNonZeroCode: shouldThrowOnNonZeroCode);

            simpleProcess.Start();
            simpleProcess.WaitForExit();
            return outputBuilder.ToString();
        }

        public static async Task<string> StartThenWaitForExitThenReadOutputAsync(
          SimpleProcessStartInfo startInfo,
          bool echoCommand = false,
          string? commandEchoPrefix = null,
          Action<string?>? errorReceived = null,
          bool shouldStreamError = false,
          bool shouldThrowOnNonZeroCode = false)
        {
            StringBuilder outputBuilder = new StringBuilder();

            using var process = new SimpleAsyncProcess(
                startInfo,
                echoCommand,
                commandEchoPrefix,
                output => outputBuilder.Append(output),
                errorReceived: errorReceived,
                shouldStreamError: shouldStreamError,
                shouldThrowOnNonZeroCode: shouldThrowOnNonZeroCode);

            process.Start();
            _ = await process.WaitForExitAsync();
            return outputBuilder.ToString();
        }

        #endregion

        private readonly StringBuilder _errorBuilder;
        private readonly ProcessStartInfo _processStartInfo;
        private bool _isDisposed;

        public bool EchoCommand { get; }
        public string? CommandEchoPrefix { get; }
        [MemberNotNullWhen(true, "Process")]
        public bool HasProcessStarted { get; private set; }
        public bool ShouldStreamOutput { get; }
        public bool ShouldStreamError { get; }
        public bool ShouldThrowOnNonZeroCode { get; }

        public int ExitCode {
            get {
                EnsureProcessCreated();
                return Process.ExitCode;
            }
        }

        protected Process? Process { get; private set; }
        private Action<string?>? _outputReceived { get; }
        private Action<string?>? _errorReceived { get; }

        // ISSUE: Streaming does not work in Ubuntu.
        public SimpleProcess(
          SimpleProcessStartInfo processStartInfo,
          bool echoCommand = false,
          string? commandEchoPrefix = null,
          Action<string?>? outputReceived = null,
          bool shouldStreamOutput = false,
          Action<string?>? errorReceived = null,
          bool shouldStreamError = false,
          bool shouldThrowOnNonZeroCode = false)
        {
            if (processStartInfo == null) {
                throw new ArgumentNullException(nameof(processStartInfo));
            }

            _errorBuilder = new StringBuilder();
            _processStartInfo = processStartInfo.ProcessStartInfo;
            EchoCommand = echoCommand;
            CommandEchoPrefix = commandEchoPrefix;
            _outputReceived = outputReceived;
            ShouldStreamOutput = shouldStreamOutput;
            _errorReceived = errorReceived;
            ShouldStreamError = shouldStreamError;
            ShouldThrowOnNonZeroCode = shouldThrowOnNonZeroCode;
        }

        [MemberNotNull("Process")]
        private void AttachProcessHandlers()
        {
            EnsureProcessCreated();
            Process.Exited += OnProcessExited;

            if (ShouldStreamOutput) {
                Process.OutputDataReceived += OnOutputDataReceived;
            }

            if (!ShouldStreamError) {
                return;
            }

            Process.ErrorDataReceived += OnErrorDataReceived;
        }

        [MemberNotNull("Process")]
        private void DettachProcessHandlers()
        {
            EnsureProcessCreated();
            Process.Exited -= OnProcessExited;

            if (ShouldStreamOutput) {
                Process.OutputDataReceived -= OnOutputDataReceived;
            }

            if (!ShouldStreamError) {
                return;
            }

            Process.ErrorDataReceived -= OnOutputDataReceived;
        }

        protected virtual void OnProcessExited(object? sender, EventArgs e) =>
            DettachProcessHandlers();

        protected void ReceiveOutput(string? data) =>
            _outputReceived?.Invoke(data);

        private void OnOutputDataReceived(object? sender, DataReceivedEventArgs e) =>
            ReceiveOutput(e.Data);

        protected void ReceiveError(string? data)
        {
            _errorBuilder?.Append(data);
            _errorReceived?.Invoke(data);
        }

        private void OnErrorDataReceived(object? sender, DataReceivedEventArgs e) =>
            ReceiveError(e.Data);

        /// <summary>Prepares the process.</summary>
        /// <exception cref="InvalidOperationException">Process alreay created.</exception>
        [MemberNotNull("Process")]
        protected virtual void CreateProcess()
        {
            if (Process is not null) {
                throw new InvalidOperationException("Process alreay created.");
            }

            Process = new Process() {
                StartInfo = _processStartInfo
            };

            Process.EnableRaisingEvents = true;
            AttachProcessHandlers();
        }

        protected NonZeroExitCodeException CreateNonZeroExitCodeException()
        {
            EnsureProcessCreated();
            bool isErrorEmpty = _errorBuilder.Length == 0;
            _errorBuilder.Insert(0, ProcessStartInfoUtils.GetExecutionInfoText(_processStartInfo, CommandEchoPrefix) + (isErrorEmpty ? "" : Environment.NewLine));
            return new NonZeroExitCodeException(Process.ExitCode, _errorBuilder.ToString());
        }

        /// <summary>When an error occurred during process start.</summary>
        protected virtual void OnProcessNotStarted(Exception error)
        {
        }

        private static Exception CreateProcessNotStartedException() => new ProcessNotSpawnedException();

        /// <summary>Starts the process.</summary>
        /// <exception cref="InvalidOperationException">Process already created.</exception>
        public void Start()
        {
            CreateProcess();
            bool isProcessStarted;

            try {
                isProcessStarted = Process.Start(EchoCommand, CommandEchoPrefix);
            } catch (Exception ex) {
                OnProcessNotStarted(ex);
                throw;
            }

            if (!isProcessStarted) {
                OnProcessNotStarted(CreateProcessNotStartedException());
            }

            if (ShouldStreamOutput) {
                Process.BeginOutputReadLine();
            }

            if (ShouldStreamError) {
                Process.BeginErrorReadLine();
            }

            HasProcessStarted = true;
        }

        /// <summary>Ensures process has been created.</summary>
        /// <exception cref="InvalidOperationException">Process not created yet.</exception>
        [MemberNotNull("Process")]
        protected void EnsureProcessCreated()
        {
            if (Process == null) {
                throw new InvalidOperationException("Process not created yet.");
            }
        }

        /// <summary>Ensures process has been started.</summary>
        /// <exception cref="InvalidOperationException">Process not started yet.</exception>
        [MemberNotNull("Process")]
        protected void EnsureProcessStarted()
        {
            if (!HasProcessStarted) {
                throw new InvalidOperationException("Process not started yet.");
            }
        }

        /// <summary>Throws if exit code is non-zero.</summary>
        /// <exception cref="NonZeroExitCodeException"></exception>
        protected void ThrowOnNonZeroExitCode()
        {
            if (!ShouldThrowOnNonZeroCode) {
                return;
            }

            EnsureProcessCreated();

            if (Process.ExitCode != 0) {
                throw CreateNonZeroExitCodeException();
            }
        }

        /// <summary>Waits for exit.</summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NonZeroExitCodeException"></exception>
        public int WaitForExit()
        {
            EnsureProcessStarted();

            if (!ShouldStreamOutput) {
                ReceiveOutput(Process.StandardOutput.ReadToEnd());
            }

            if (!ShouldStreamError) {
                ReceiveError(Process.StandardError.ReadToEnd());
            }

            Process.WaitForExit();
            ThrowOnNonZeroExitCode();
            return Process.ExitCode;
        }

        public void Kill()
        {
            EnsureProcessStarted();
            Process.Kill();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) {
                return;
            }

            if (disposing) {
                DettachProcessHandlers();
                Process.Dispose();
            }

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
