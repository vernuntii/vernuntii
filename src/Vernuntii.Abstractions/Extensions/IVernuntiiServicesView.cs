using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// The features for the <see cref="Vernuntii"/>-library.
    /// </summary>
    public interface IVernuntiiServicesView
    {
        /// <summary>
        /// The services.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
