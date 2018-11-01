using System.Collections.Generic;

using Newtonsoft.Json;

namespace CommonObjects.Models
{
    public class UserListResponse : Response
    {
        [JsonProperty("users")]
        public List<User> Users { get; set; }

        public UserListResponse(List<User> users) : base("user_list")
        {
            Users = users;
        }
    }
}
