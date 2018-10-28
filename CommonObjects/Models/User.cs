using System;
using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class User
    {
        [JsonProperty("id")]
        public String ID { get; set; }

        [JsonProperty("name")]
        public String Name { get; set; }

        public User(String name, String id = null)
        {
            ID = id ?? Guid.NewGuid().ToString();
            Name = name;
        }
    }
}
