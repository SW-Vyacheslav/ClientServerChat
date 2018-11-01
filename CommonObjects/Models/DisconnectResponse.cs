using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class DisconnectResponse : Response
    {
        [JsonProperty("user")]
        public User @User { get; set; }

        public DisconnectResponse(User user) : base("disconnect")
        {
            User = user;
        }
    }
}
