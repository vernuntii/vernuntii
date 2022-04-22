using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Autofac
{
    public interface ILifetimeScopedServiceProvider : IServiceProvider, ISupportRequiredService, IDisposable, IAsyncDisposable
    {
        /// <inheritdoc/>
        ILifetimeScope LifetimeScope { get; }
    }
}
