using System;
using Newtonsoft.Json;

namespace CommonObjects.Models
{
    [Serializable]
    public class User
    {
        [JsonProperty("id")]
        public String ID { get; set; }

        [JsonProperty("name")]
        public String Name { get; set; }

        [JsonIgnore]
        public String Password { get; set; }

        [JsonIgnore]
        public Boolean IsBanned { get; set; }

        [JsonIgnore]
        public Boolean IsConnected { get; set; }

        public User(String id, String name, String password)
        {
            ID = id;
            Name = name;
            Password = password;
            IsBanned = false;
            IsConnected = false;
        }
    }
}
