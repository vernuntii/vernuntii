using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Autofac
{
    /// <summary>
    /// Represents a full functional <see cref="IServiceProvider"/> provided
    /// by <see cref="Autofac"/> through <see cref="LifetimeScope"/>.
    /// </summary>
    public interface ILifetimeScopedServiceProvider : IServiceProvider, ISupportRequiredService, IDisposable, IAsyncDisposable
    {
        /// <inheritdoc/>
        ILifetimeScope LifetimeScope { get; }
    }
}
