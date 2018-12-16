using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class UserSignOutResponse : Response
    {
        [JsonProperty("user")]
        public User @User { get; set; }

        public UserSignOutResponse(User user) : base("user_signout")
        {
            User = user;
        }
    }
}
