using System;

using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class AuthRequest : Request
    {
        [JsonProperty("user_name")]
        public String UserName { get; set; }

        [JsonProperty("password")]
        public String Password { get; set; }

        public AuthRequest(String user_name, String password) : base("auth")
        {
            UserName = user_name;
            Password = password;
        }
    }
}
