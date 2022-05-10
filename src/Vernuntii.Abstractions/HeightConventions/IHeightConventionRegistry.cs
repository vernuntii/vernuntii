using Vernuntii.Collections;

namespace Vernuntii.HeightConventions
{
    /// <summary>
    /// A registry for items of type <see cref="IHeightConvention"/>.
    /// </summary>
    public interface IHeightConventionRegistry : INamedItemRegistry<IHeightConvention>
    {
    }
}
