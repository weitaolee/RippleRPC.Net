using Newtonsoft.Json;

namespace RippleRPC.Net.Model
{
    public class TransactionRecord
    {
        [JsonProperty("meta")]
        public TransactionRecordMeta Meta { get; set; }

        [JsonProperty("tx")]
        public TransactionRecordDetail TransactionDetail { get; set; }

        [JsonProperty("validated")]
        public bool Validated { get; set; }
    }
}
