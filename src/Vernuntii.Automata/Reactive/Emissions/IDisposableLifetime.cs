namespace Vernuntii.Reactive.Emissions;

public interface IDisposableLifetime : IDisposable
{
    bool IsDisposed { get; }
}
