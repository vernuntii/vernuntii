using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Vernuntii.Configuration.Queueing;

namespace Vernuntii.Configuration.Json
{
    internal class JsonShadowedConfigurationSource : JsonConfigurationSource
    {
        public Action<IConfigurationProviderQueue>? OnBuildShadowedConfigurationProvider { get; set; }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var configurationBuilder = new QueuedConfigurationProviderBuilder(base.Build(builder));
            OnBuildShadowedConfigurationProvider?.Invoke(configurationBuilder);
            return configurationBuilder.Build();
        }
    }
}
