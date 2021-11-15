using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace EliteAPI.Dashboard.Plugins.Installer
{
    public class Plugin
    {
        [JsonProperty("name")]
        public string Name { get; private set; }
        
        [JsonProperty("description")]
        public string Description { get; private set; }
        
        [JsonProperty("repository")]
        public string Repository { get; private set; }
        
        [JsonProperty("icon")]
        public string Icon { get; private set; }

        [JsonProperty("defaultInstallationPath")] 
        public string RawDefaultInstallationPath { get; private set; }

        [JsonProperty("processedDefaultInstallationPath")] 
        public string DefaultInstallationPath => DefaultInstallationDirectory().FullName;
        
        [JsonProperty("isInstalled")] 
        public bool IsInstalled { get; internal set; } = false;
        
        [JsonProperty("latestVersion")] 
        public string LatestVersion { get; internal set; } = "0.0.0";
        
        [JsonProperty("installedVersion")] 
        public string InstalledVersion { get; internal set; } = "0.0.0";
        
        [JsonProperty("installationPath")] 
        public string InstallationPath { get; internal set; }
        
        private DirectoryInfo DefaultInstallationDirectory()
        {
            var path = RawDefaultInstallationPath;
            
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EliteAPI", "Plugins", Name);
            }
            
            // Special folders
            foreach (var folder in Enum.GetValues(typeof(Environment.SpecialFolder)))
            {
                var folderName = Enum.GetName(typeof(Environment.SpecialFolder), folder);
                path = path.Replace($"[{folderName}]", Environment.GetFolderPath((Environment.SpecialFolder)folder), StringComparison.InvariantCultureIgnoreCase);
            }
            
            // Environment variables
            var reg = new Regex(@"\[(.*?),(.*?)\]");
            var matches = reg.Matches(path);
            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;
                var name = match.Groups[2].Value;
                
                var value = Registry.GetValue(key.Replace('/', '\\'), name, null);

                path = path.Replace($"[{key},{name}]", value != null ? value.ToString() : Environment.GetFolderPath(Environment.SpecialFolder.Desktop), StringComparison.InvariantCultureIgnoreCase);
            }

            return new DirectoryInfo(path);
        }
    }
}