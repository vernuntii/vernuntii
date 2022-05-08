using Vernuntii.Collections;

namespace Vernuntii.MessageConventions
{
    /// <summary>
    /// A registry for items of type <see cref="IMessageConvention"/>.
    /// </summary>
    public interface IMessageConventionRegistry : INamedItemRegistry<IMessageConvention?>
    {
    }
}
