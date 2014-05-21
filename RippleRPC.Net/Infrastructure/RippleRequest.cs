using System;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

namespace RippleRPC.Net.Infrastructure
{
    [JsonObject(MemberSerialization = MemberSerialization.Fields)]
    public class RippleRequest
    {
        [JsonProperty(PropertyName = "method")] 
        string method;
        
        [JsonProperty(PropertyName="params", NullValueHandling = NullValueHandling.Ignore)]
        List<ExpandoObject> requestParams = null;

        public RippleRequest(string method, List<ExpandoObject> requestParams = null)
        {
            this.method = method;
            this.requestParams = requestParams;                
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
        
    }
}
