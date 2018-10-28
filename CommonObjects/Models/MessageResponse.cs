using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class MessageResponse : Response
    {
        [JsonProperty("message")]
        public Message @Message { get; set; }

        public MessageResponse(Message message) : base("message")
        {
            Message = message;
        }
    }
}
