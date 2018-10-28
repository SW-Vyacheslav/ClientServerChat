using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class DisconnectRequest : Request
    {
        [JsonProperty("user")]
        public User @User { get; set; }

        public DisconnectRequest(User user) : base("disconnect")
        {
            User = user;
        }
    }
}
