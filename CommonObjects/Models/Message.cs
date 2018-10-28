using System;
using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class Message
    {
        [JsonProperty("from")]
        public User From { get; set; }

        [JsonProperty("text")]
        public String Text { get; set; }

        public Message(User from, String text)
        {
            From = from;
            Text = text;
        }
    }
}
