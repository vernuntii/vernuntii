namespace Vernuntii.Diagnostics
{
    internal interface ISimpleAsyncProcess : IDisposable
    {
        string? CommandEchoPrefix { get; }
        bool EchoCommand { get; }
        bool HasProcessStarted { get; }

        void Start();
        Task<int> WaitForExitAsync();
        void Kill();
    }
}
