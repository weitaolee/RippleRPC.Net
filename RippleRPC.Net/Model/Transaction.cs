using Newtonsoft.Json;

namespace RippleRPC.Net.Model
{
    public class Transaction
    {
        [JsonProperty("meta")]
        public TransactionMeta Meta { get; set; }

        [JsonProperty("tx")]
        public TransactionDetail TransactionDetail { get; set; }

        [JsonProperty("validated")]
        public bool Validated { get; set; }
    }
}
