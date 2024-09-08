using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive.Disposables;

/// <summary>
/// A collection with instances of type <see cref="IDisposable"/>.
/// </summary>
internal class DisposableCollection : IDisposableLifetime, IEnumerable<IDisposable>
{
    public static DisposableCollection Of(params IDisposable[] disposables) =>
        new DisposableCollection(disposables);

    public bool IsDisposed =>
        _disposables is null;

    private List<IDisposable>? _disposables;

    public DisposableCollection() =>
        _disposables = new List<IDisposable>();

    public DisposableCollection(IEnumerable<IDisposable> disposables) =>
        _disposables = new List<IDisposable>(disposables);

    public void Add(IDisposable disposable)
    {
        var disposables = _disposables;

        if (disposables is null) {
            throw new ObjectDisposedException(null);
        }

        lock (disposables) {
            if (!IsDisposed) {
                disposables.Add(disposable);
                return;
            }
        }

        // Else
        throw new ObjectDisposedException(null);
    }

    public bool TryAddOrDispose(IDisposable disposable)
    {
        var disposables = _disposables;

        if (disposables is null) {
            disposable.Dispose();
            return false;
        }

        lock (disposables) {
            if (!IsDisposed) {
                disposables.Add(disposable);
                return true;
            }
        }

        // Else
        disposable.Dispose();
        return false;
    }

    public bool TryAdd(IDisposable disposable)
    {
        var disposables = _disposables;

        if (disposables is null) {
            return false;
        }

        lock (disposables) {
            if (!IsDisposed) {
                disposables.Add(disposable);
                return true;
            }
        }

        // Else
        return false;
    }

    public bool TryAdd<T>(Func<T> disposableFactory, [NotNullWhen(true)] out T? disposable)
        where T : IDisposable
    {
        var disposables = _disposables;

        if (disposables is null) {
            disposable = default;
            return false;
        }

        lock (disposables) {
            if (!IsDisposed) {
                disposable = disposableFactory();
                disposables.Add(disposable);
                return true;
            }
        }

        // Else
        disposable = default;
        return false;
    }

    private void DisposeCollection()
    {
        var disposables = _disposables;

        if (disposables is null) {
            return;
        }

        IDisposable[] disposablesCopy;

        lock (disposables) {
            if (IsDisposed) {
                return;
            }

            disposablesCopy = new IDisposable[disposables.Count];
            disposables.CopyTo(disposablesCopy);
            disposables.Clear();
        }

        foreach (var disposable in disposablesCopy) {
            disposable.Dispose();
        }
    }

    public void Dispose()
    {
        var disposables = _disposables;

        if (disposables is null) {
            return;
        }

        lock (disposables) {
            if (_disposables is null) {
                return;
            }

            _disposables = null;
        }

        foreach (var disposable in disposables) {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Disposes all so far added disposables and removes them from the collection.
    /// </summary>
    /// <param name="permanently">
    /// If true, then this instance will be ultimately disposed.
    /// </param>
    public void Dispose(bool permanently)
    {
        if (permanently) {
            Dispose();
        } else {
            DisposeCollection();
        }
    }

    /// <summary>
    /// Gets the snapshot of current disposables.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public IEnumerator<IDisposable> GetEnumerator()
    {
        var disposables = _disposables;

        if (disposables is null) {
            throw new ObjectDisposedException(null);
        }

        IDisposable[] disposablesCopy;

        lock (disposables) {
            if (IsDisposed) {
                throw new ObjectDisposedException(null);
            }

            disposablesCopy = new IDisposable[disposables.Count];
            disposables.CopyTo(disposablesCopy);
            disposables.Clear();
        }

        return ((IEnumerable<IDisposable>)disposablesCopy).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
