using Newtonsoft.Json;

namespace RippleRPC.Net.Model
{
    public class CurrencyItem
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("issuer", NullValueHandling = NullValueHandling.Ignore)]
        public string Issuer { get; set; }        
    }
}
