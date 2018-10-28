using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class ConnectResponse : Response
    {
        [JsonProperty("user")]
        public User @User { get; set; }

        public ConnectResponse(User user) : base("connect")
        {
            User = user;
        }
    }
}
