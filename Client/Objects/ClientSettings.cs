using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Extensions.Configuration.Ini;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Client.Objects
{
    public class ClientSettings
    {
        public IPAddress ServerIPAddress { get; set; }
        public String UserName { get; set; }

        private ConfigurationBuilder _configurationBuilder;

        private String _iniFilePath = Environment.CurrentDirectory + "\\" + Resources.Resources.ini_file_name;

        public ClientSettings()
        {
            if (!File.Exists(_iniFilePath))
            {
                using (StreamWriter streamWriter = File.CreateText(_iniFilePath))
                {
                    streamWriter.Write(Resources.Resources.config);
                }
            }

            _configurationBuilder = new ConfigurationBuilder();
            _configurationBuilder.AddIniFile(_iniFilePath);

            IConfiguration configuration = _configurationBuilder.Build();
            IConfigurationSection configurationSection = configuration.GetSection("Settings");

            foreach (var item in configurationSection.GetChildren())
            {
                if (item.Key == "server_ip") ServerIPAddress = IPAddress.Parse(item.Value);
                if (item.Key == "user_name") UserName = item.Value;
            } 
        }

        public void SaveSettingsToFile()
        {
            String[] file_data = File.ReadAllLines(_iniFilePath);

            for (int i = 0; i < file_data.Length; i++)
            {
                if(file_data[i].Contains("server_ip"))
                {
                    file_data[i] = file_data[i].Substring(file_data[i].IndexOf("server_ip"),10) + ServerIPAddress.ToString();
                }
                if (file_data[i].Contains("user_name"))
                {
                    file_data[i] = file_data[i].Substring(file_data[i].IndexOf("user_name"), 10) + UserName;
                }
            }

            File.WriteAllLines(_iniFilePath,file_data);
        }
    }
}
