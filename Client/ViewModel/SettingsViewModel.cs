using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Client.Objects;
using CommonObjects.Helpers;
using CommonObjects.Models;

namespace Client.ViewModel
{
    public class SettingsViewModel : ObservableObject
    {
        private const String _data_file_name = "data.bin";

        public Settings @Settings { get; private set; }

        public SettingsViewModel()
        {
            GetSettings();
        }

        private void GetSettings()
        {
            String data_file_path = Environment.CurrentDirectory + "/" + _data_file_name;
            BinaryFormatter bin_formatter = new BinaryFormatter();

            if (!File.Exists(data_file_path))
            {
                Settings = new Settings(new User());
                using (FileStream file_stream = new FileStream(data_file_path, FileMode.Create, FileAccess.Write))
                {
                    bin_formatter.Serialize(file_stream, Settings);
                }
            }
            else
            {
                using (FileStream file_stream = new FileStream(data_file_path,FileMode.Open,FileAccess.Read))
                {
                    Settings = bin_formatter.Deserialize(file_stream) as Settings;
                }
            }
        }

        public void ApplySettings()
        {
            String data_file_path = Environment.CurrentDirectory + "/" + _data_file_name;
            BinaryFormatter bin_formatter = new BinaryFormatter();

            using (FileStream file_stream = new FileStream(data_file_path, FileMode.Create, FileAccess.Write))
            {
                bin_formatter.Serialize(file_stream, Settings);
            }
        }
    }
}