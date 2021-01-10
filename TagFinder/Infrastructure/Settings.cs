using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace TagFinder.Infrastructure
{
    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool CheckForUpdates { get; set; } = true;

        public static Settings Load()
        {
            if (!File.Exists(FilePath.SETTINGS_FILE))
            {
                var settings = new Settings();
                settings.Save();
                return settings;
            }

            try
            {
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath.SETTINGS_FILE));
            }
            catch (System.Exception)
            {
                Program.Logger.Log("[ERROR] Failed to read settings from file. Loading defaults.");
                return new Settings();
            }
        }

        public void Save()
        {
            string jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });

            try
            {
                File.WriteAllText(FilePath.SETTINGS_FILE, jsonString);
            }
            catch (System.Exception ex)
            {
                Program.Logger.Log("[ERROR] Failed to save settings: " + ex.Message);
            }

        }
    }
}
