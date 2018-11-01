using System;
using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public abstract class Response
    {
        [JsonProperty("ok")]
        public Boolean Ok { get; set; }

        [JsonProperty("error")]
        public String Error { get; set; }

        [JsonProperty("response_type")]
        public String ResponseType { get; private set; }

        public Response(String response_type, Boolean ok = true, String error = null)
        {
            ResponseType = response_type;
            Error = error;
            Ok = ok;
        }
    }
}
