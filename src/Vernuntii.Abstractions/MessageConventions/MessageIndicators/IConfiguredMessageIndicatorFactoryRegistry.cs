using Vernuntii.Collections;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// A registry for items of type <see cref="IMessageIndicator"/>.
    /// </summary>
    public interface IConfiguredMessageIndicatorFactoryRegistry : INamedItemRegistry<IConfiguredMessageIndicatorFactory>
    {
    }
}
