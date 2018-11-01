using System;

using CommonObjects.Models;

namespace Client.Objects
{
    [Serializable]
    public class Settings
    {
        public User User { get; set; }
        public String ServerIPAddress { get; set; }

        public Settings(User user, String server_ip = "127.0.0.1")
        {
            User = user;
            ServerIPAddress = server_ip;
        }
    }
}
