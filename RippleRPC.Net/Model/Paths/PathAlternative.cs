using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RippleRPC.Net.Model.Paths
{
    public class PathAlternative
    {
        [JsonProperty("paths_canonical")]
        [JsonConverter(typeof(PathConverter))]
        public List<object> CanonicalPaths { get; set; }

        [JsonProperty("paths_computed")]
        [JsonConverter(typeof(PathConverter))]
        public List<object> ComputedPaths { get; set; }

        [JsonProperty("source_amount")]
        public RippleCurrencyValue SourceAmount { get; set; }
    }
}
