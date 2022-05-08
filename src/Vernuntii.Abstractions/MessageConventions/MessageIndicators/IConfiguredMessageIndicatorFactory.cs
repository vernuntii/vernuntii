using Microsoft.Extensions.Configuration;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// A factory for message indicator.
    /// </summary>
    public interface IConfiguredMessageIndicatorFactory
    {
        /// <summary>
        /// Creates a message indicator.
        /// </summary>
        /// <param name="indicatorName"></param>
        /// <param name="versionPart"></param>
        /// <param name="indicatorConfiguration"></param>
        IMessageIndicator Create(IConfiguration indicatorConfiguration, string indicatorName, VersionPart versionPart);
    }
}
