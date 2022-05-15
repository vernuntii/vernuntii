using Vernuntii.MessagesProviders;
using Vernuntii.VersionTransformers;

namespace Vernuntii.VersionIncrementing
{
    internal interface IVersionIncrementBuilder
    {
        IEnumerable<IVersionTransformer> BuildIncrement(IMessage message, VersionIncrementContext context);
    }
}
