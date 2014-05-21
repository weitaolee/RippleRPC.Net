using Newtonsoft.Json;

namespace RippleRPC.Net.Model
{
    public class OfferItem : CurrencyItem
    {
        [JsonProperty("value")]
        public double Value { get; set; }
    }
}
