using Vernuntii.MessagesProviders;
using Vernuntii.VersionTransformers;

namespace Vernuntii.VersionIncrementing
{
    internal interface IVersionIncrementBuilder
    {
        IEnumerable<ISemanticVersionTransformer> BuildIncrement(IMessage message, VersionIncrementContext context);
    }
}
