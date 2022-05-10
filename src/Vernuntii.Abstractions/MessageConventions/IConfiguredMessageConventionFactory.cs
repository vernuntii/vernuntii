using System.Diagnostics.CodeAnalysis;
using Vernuntii.Configuration;

namespace Vernuntii.MessageConventions
{
    /// <summary>
    /// A factory for <see cref="IMessageConvention"/>.
    /// </summary>
    public interface IConfiguredMessageConventionFactory
    {
        /// <summary>
        /// A factory for <see cref="IMessageConvention"/>.
        /// </summary>
        /// <param name="sectionProvider"></param>
        /// <param name="messageConvention"></param>
        bool TryCreate(IDefaultConfigurationSectionProvider sectionProvider, [NotNullWhen(true)] out IMessageConvention? messageConvention);
    }
}
