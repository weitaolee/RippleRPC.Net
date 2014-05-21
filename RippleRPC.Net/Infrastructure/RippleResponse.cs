using Newtonsoft.Json;
using RippleRPC.Net.Model;

namespace RippleRPC.Net.Infrastructure
{
    public class RippleResponse<T>
    {
        [JsonProperty("result")]
        public T Result { get; set; }
        public RippleError Error { get; set; }

        [JsonProperty("ledger_current_index")]
        public long LedgerCurrentIndex { get; set; }
        
        [JsonProperty("status")]
        public string Status { get; set; }

    }

}
