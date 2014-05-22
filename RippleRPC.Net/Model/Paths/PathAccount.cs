using Newtonsoft.Json;

namespace RippleRPC.Net.Model.Paths
{
    public class PathAccount
    {
        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("type_hex")]
        public string TypeHex { get; set; }
    }
}
