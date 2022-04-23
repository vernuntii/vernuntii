using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace Vernuntii.Autofac
{
    /// <summary>
    /// Represents a full functional <see cref="IServiceProvider"/> provided
    /// by <see cref="Autofac"/> through <see cref="ILifetimeScopedServiceProvider.LifetimeScope"/>.
    /// </summary>
    public sealed class LifetimeScopedServiceProvider : AutofacServiceProvider, ILifetimeScopedServiceProvider
    {
        /// <summary>
        /// Creates an instance of this.
        /// </summary>
        /// <param name="lifetimeScope">The container</param>
        public LifetimeScopedServiceProvider(ILifetimeScope lifetimeScope)
            : base(lifetimeScope)
        {
        }
    }
}
