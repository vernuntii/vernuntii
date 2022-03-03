using System;

namespace Vernuntii.Git.Diagnostics
{
    internal interface ISimpleProcess : IDisposable
    {
        string? CommandEchoPrefix { get; }
        bool EchoCommand { get; }
        bool HasProcessStarted { get; }

        void Start();
        int WaitForExit();
        void Kill();
    }
}
