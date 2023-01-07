namespace Vernuntii.Reactive;

public interface IDisposableLifetime : IDisposable
{
    bool IsDisposed { get; }
}
