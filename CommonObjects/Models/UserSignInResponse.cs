using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class UserSignInResponse : Response
    {
        [JsonProperty("user")]
        public User @User { get; set; }

        public UserSignInResponse(User user) : base("user_signin")
        {
            User = user;
        }
    }
}
