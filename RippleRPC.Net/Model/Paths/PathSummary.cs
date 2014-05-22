using System.Collections.Generic;
using Newtonsoft.Json;

namespace RippleRPC.Net.Model.Paths
{
    public class PathSummary
    {
        [JsonProperty("alternatives")]
        public List<PathAlternative> Alternatives { get; set; }

        [JsonProperty("destination_account")]
        public string DestinationAccount { get; set; }

        [JsonProperty("destination_currencies")]
        public List<string> DestinationCurrencies { get; set; }
    }
}
