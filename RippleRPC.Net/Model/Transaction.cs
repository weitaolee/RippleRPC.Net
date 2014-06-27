using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RippleRPC.Net.Crypto;
using RippleRPC.Net.Crypto.Encodings;

namespace RippleRPC.Net.Model
{
    public class Transaction
    {
        [JsonProperty]
        public TransactionType TransactionType { get; set; }
        
        [JsonProperty]
        public string Account { get; set; }

        [JsonProperty]
        public string Destination { get; set; }

        public List<object> Paths { get; set; }

        [JsonProperty]
        public RippleCurrencyValue Amount { get; set; }

        public RippleCurrencyValue SendMax { get; set; }

        [JsonIgnore]
        public bool Signed { get { return !string.IsNullOrEmpty(Hash); } }

        [JsonIgnore]
        public string Hash { get; set; }

        public void Sign(string secret)
        {
            string s = ToJson();
            byte[] bytes = Seed.PassPhraseToSeedBytes(secret);
            KeyPair keyPair = (KeyPair) Seed.CreateKeyPair(bytes);
            byte[] transaction = Encoding.ASCII.GetBytes(s);
            byte[] signedTransaction = keyPair.Sign(transaction);
            Hash = B16.ToString(signedTransaction);
        }

        public string ToJson()
        {
            var transaction = "{" +  
            "\"TransactionType\": \"Payment\", " +
            "\"Account\": \"rho3u4kXc5q3chQFKfn9S1ZqUCya1xT3t4\"," + 
            "\"Destination\": \"rDLXQ8KEBn3Aw313bGzhEemx8cCPpGha3d\"," + 
            "\"Amount\": \"1000\", " +
            "\"Fee\": \"10\", " +
            "\"Sequence\": 59 }";
            return transaction;
        }
    }
}
