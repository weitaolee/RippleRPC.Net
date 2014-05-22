using Newtonsoft.Json;

namespace RippleRPC.Net.Model
{
    public class RippleCurrency
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("issuer", NullValueHandling = NullValueHandling.Ignore)]
        public string Issuer { get; set; }        
    }

    public class RippleCurrencyValue : RippleCurrency
    {
        [JsonProperty("value")]
        public double Value { get; set; }
    }
}
