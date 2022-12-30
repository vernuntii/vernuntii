using System.Text.Json.Serialization;

namespace Vernuntii.VersionCaching
{
    [JsonSerializable(typeof(DefaultVersionCache))]
    internal partial class VersionCacheSerializerContext : JsonSerializerContext
    {
    }
}
