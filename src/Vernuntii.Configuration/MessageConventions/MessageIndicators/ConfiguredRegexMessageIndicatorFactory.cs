using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// A factory for RegEx message indicator.
    /// </summary>
    public class ConfiguredRegexMessageIndicatorFactory : IConfiguredMessageIndicatorFactory
    {
        /// <summary>
        /// The default instance.
        /// </summary>
        public static readonly ConfiguredRegexMessageIndicatorFactory Default = new();

        /// <inheritdoc/>
        public IMessageIndicator Create(IConfiguration indicatorConfiguration, string indicatorName, VersionPart versionPart)
        {
            var indicatorObject = indicatorConfiguration.Get<IndicatorObject>();

            if (indicatorObject?.Pattern == null) {
                throw new ConfigurationValidationException($"Regular expression pattern cannot be null (Message indicator = {indicatorName})");
            }

            return RegexMessageIndicator.Empty.With.PartRegex(new Regex(indicatorObject.Pattern), versionPart).ToIndicator();
        }

        private class IndicatorObject
        {
            public string? Pattern { get; set; }
        }
    }
}
