using System;
using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public abstract class Request
    {
        [JsonProperty("request_type")]
        public String RequestType { get; private set; }

        public Request(String request_type)
        {
            RequestType = request_type;
        }
    }
}
