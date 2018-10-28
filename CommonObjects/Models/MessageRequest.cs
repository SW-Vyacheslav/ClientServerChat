using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class MessageRequest : Request
    {
        [JsonProperty("message")]
        public Message @Message { get; set; }

        public MessageRequest(Message message) : base("message")
        {
            Message = message;
        }
    }
}
