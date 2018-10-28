using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class ConnectRequest : Request
    {
        [JsonProperty("user")]
        public User @User { get; set; }

        public ConnectRequest(User user) : base("connect")
        {
            User = user;
        }
    }
}
