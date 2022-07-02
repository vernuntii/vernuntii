using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vernuntii.VersionCaching
{
    [JsonSerializable(typeof(DefaultVersionCache))]
    internal partial class VersionCacheSerializerContext : JsonSerializerContext
    {
    }
}
