using Vernuntii.MessagesProviders;
using Vernuntii.VersionTransformers;

namespace Vernuntii.MessageVersioning
{
    internal interface IVersionIncrementBuilder
    {
        IEnumerable<ISemanticVersionTransformer> BuildIncrement(IMessage message, MessageVersioningContext context);
    }
}