using System;
using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public abstract class Response
    {
        [JsonProperty("response_type")]
        public String ResponseType { get; private set; }

        public Response(String response_type)
        {
            ResponseType = response_type;
        }
    }
}
