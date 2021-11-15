using System;
using System.Collections.Generic;
using System.IO;
using EliteAPI.Dashboard.Plugins.Installer;
using Newtonsoft.Json;

namespace EliteAPI.Dashboard
{
    public class UserProfile
    {
        public static string SaveFolderPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EliteAPI");

        private static string SaveFilePath = Path.Combine(SaveFolderPath, "userprofile.json");

        public void Save()
        {
            Directory.CreateDirectory(SaveFolderPath);
            File.WriteAllText(SaveFilePath, JsonConvert.SerializeObject(this));
        }

        public static UserProfile Get()
        {
            try {
                return File.Exists(SaveFilePath)
                ? JsonConvert.DeserializeObject<UserProfile>(File.ReadAllText(SaveFilePath))
                : new UserProfile();
            } catch(Exception) {
                return new UserProfile();
            }
        }

        public static void Set(UserProfile userProfile)
        {
            Directory.CreateDirectory(SaveFolderPath);
            File.WriteAllText(SaveFilePath, JsonConvert.SerializeObject(userProfile));
        }

        public static void Set(string json)
        {
            Directory.CreateDirectory(SaveFolderPath);
            File.WriteAllText(SaveFilePath,
                JsonConvert.SerializeObject(JsonConvert.DeserializeObject<UserProfile>(json)));
        }

        [JsonProperty("plugins")]
        public IList<Plugin> Plugins { get; init; } = new List<Plugin>();

        [JsonProperty("firstRun")]
        public bool FirstRun { get; init; } = true;
    }
}