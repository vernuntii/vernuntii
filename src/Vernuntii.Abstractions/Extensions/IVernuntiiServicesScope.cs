using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// The features for the <see cref="Vernuntii"/>-library.
    /// </summary>
    public interface IVernuntiiServicesScope
    {
        /// <summary>
        /// The services.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
