﻿using Newtonsoft.Json;

namespace RippleRPC.Net.Model.Paths
{
    public class PathCurrency
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("issuer")]
        public string Issuer { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("type_hex")]
        public string TypeHex { get; set; }
    }
}
