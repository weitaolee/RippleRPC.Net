using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RippleRPC.Net.Model
{
    public class TransactionDetail
    {
        public string Account { get; set; }

        [JsonConverter(typeof(RippleValueConverter))]
        public double Fee { get; set; }
        public long Flags { get; set; }
        public int LastLedgerSequence { get; set; }
        public int Sequence { get; set; }
        public string SigningPubKey { get; set; }
        
        [JsonConverter(typeof(OfferItemConverter))]
        public object TakerGets { get; set; }

        [JsonConverter(typeof(OfferItemConverter))]
        public object TakerPays { get; set; }
        public string TransactionType { get; set; }
        public string TxnSignature { get; set; }
        
        [JsonProperty("date")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Date { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("inLedger")]
        public int InLedger { get; set; }

        [JsonProperty("ledger_index")]
        public int LedgerIndex { get; set; }

    }
}
