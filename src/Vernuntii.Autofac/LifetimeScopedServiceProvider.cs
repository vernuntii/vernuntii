using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace Vernuntii.Autofac
{
    public sealed class LifetimeScopedServiceProvider : AutofacServiceProvider, ILifetimeScopedServiceProvider
    {
        public LifetimeScopedServiceProvider(ILifetimeScope lifetimeScope)
            : base(lifetimeScope)
        {
        }
    }
}
