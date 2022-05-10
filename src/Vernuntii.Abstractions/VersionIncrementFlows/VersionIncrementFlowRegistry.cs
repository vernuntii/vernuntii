using Vernuntii.Collections;

namespace Vernuntii.VersionIncrementFlows
{
    /// <summary>
    /// The registry for instances of type <see cref="IVersionIncrementFlow"/>.
    /// </summary>
    public interface IVersionIncrementFlowRegistry : INamedItemRegistry<IVersionIncrementFlow>
    {
    }
}
