using System;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

namespace RippleRPC.Net.Infrastructure
{
    public class RippleRequest
    {
        public int id;

        string command;

        [JsonIgnore]
        ExpandoObject requestParams = null;

        public RippleRequest(int id, string command, ExpandoObject requestParams = null)
        {
            this.id = id;
            this.command = command;
            this.requestParams = requestParams;
        }

        public string ToJsonString()
        {
            var params_req = JsonConvert.SerializeObject(this.requestParams);

            if (!string.IsNullOrEmpty(params_req))
            {
                if (params_req.IndexOf("{") == 0) params_req = params_req.Substring(1);
                if (params_req.IndexOf("}") > -1) params_req = params_req.Substring(0, params_req.Length - 1);
                if (params_req.Length > 0) params_req = "," + params_req;
            }

            return "{\"id\":" + this.id.ToString() + ",\"command\":\"" + this.command + "\"" + params_req + "}";
        }

    }
}
