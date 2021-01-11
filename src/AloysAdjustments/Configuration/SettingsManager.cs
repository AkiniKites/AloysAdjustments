using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AloysAdjustments.Utility;
using Newtonsoft.Json;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace AloysAdjustments.Configuration
{
    public class SettingsManager
    {
        private const string SettingsFile = "settings.json";
        private static readonly string ApplicationSettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            typeof(SettingsManager).Assembly.GetName().Name);

        private static string SettingsPath => Path.Combine(ApplicationSettingsPath, SettingsFile);

        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private static UserSettings _settings;

        public static async Task<UserSettings> Load()
        {
            if (_settings != null)
                _settings.PropertyChanged -= Settings_PropertyChanged;

            if (!await FileBackup.RunWithBackup(SettingsPath, () =>
            {
                var json = File.ReadAllText(SettingsPath);
                if (String.IsNullOrEmpty(json))
                    return false;

                _settings = JsonConvert.DeserializeObject<UserSettings>(json);
                return true;
            }))
            {
                _settings = new UserSettings();
            }

            _settings.PropertyChanged += Settings_PropertyChanged;
            return _settings;
        }

        private static async void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
             await Save(_settings);
        }

        private static async Task Save(UserSettings settings)
        {
            await Async.Run(() =>
            {
                try
                {
                    _lock.Wait();

                    Paths.CheckDirectory(ApplicationSettingsPath);

                    using var backup = new FileBackup(SettingsPath);

                    var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText(SettingsPath, json);

                    backup.Delete();
                }
                finally
                {
                    _lock.Release();
                }
            });
        }

    }
}
