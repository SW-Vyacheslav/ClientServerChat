using System;

namespace Client.Objects
{
    [Serializable]
    public class Settings
    {
        public String ServerIPAddress { get; private set; }

        public Settings(String server_ip = "127.0.0.1")
        {
            ServerIPAddress = server_ip;
        }
    }
}
