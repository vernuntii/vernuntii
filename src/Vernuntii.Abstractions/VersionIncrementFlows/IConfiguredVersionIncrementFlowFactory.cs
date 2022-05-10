using System.Diagnostics.CodeAnalysis;
using Vernuntii.Configuration;

namespace Vernuntii.VersionIncrementFlows
{
    /// <summary>
    /// A factory for <see cref="IVersionIncrementFlow"/>.
    /// </summary>
    public interface IConfiguredVersionIncrementFlowFactory
    {
        /// <summary>
        /// Tries to create a version increment flow from configuration section.
        /// </summary>
        /// <param name="sectionProvider"></param>
        /// <param name="incrementFlow"></param>
        bool TryCreate(IDefaultConfigurationSectionProvider sectionProvider, [NotNullWhen(true)] out IVersionIncrementFlow? incrementFlow);
    }
}
