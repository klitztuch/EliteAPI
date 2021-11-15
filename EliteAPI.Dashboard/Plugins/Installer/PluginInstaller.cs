using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EliteAPI.Dashboard.Plugins.Installer
{
    public class PluginInstaller
    {
        private ILogger<PluginInstaller> _log;
        private readonly HttpClient _client;

        private IList<Plugin> _plugins;
        private const string Repositories = "https://github.com/EliteAPI/Repositories/raw/main/plugins.json";
        
        public PluginInstaller(ILogger<PluginInstaller> log, HttpClient client)
        {
            _log = log;
            _client = client;
            _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EliteAPI", "1.0.0"));
        }

        public async Task<IList<Plugin>> GetPlugins()
        {
            if (_plugins != null)
                return _plugins;
       
            var response = await _client.GetAsync(Repositories);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            _plugins = JsonConvert.DeserializeObject<IList<Plugin>>(content);

            return _plugins;
        }
        
        public async Task<GitHubRelease> GetLatestVersion(Plugin plugin)
        {
            // Retrieve the latest version from GitHub using the plugin's repository
            var result = await _client.GetAsync($"https://api.github.com/repos/{plugin.Repository}/releases/latest");
            result.EnsureSuccessStatusCode();
            var json = await result.Content.ReadAsStringAsync();
            var latest = JsonConvert.DeserializeObject<GitHubRelease>(json);
            return latest;
        }
        
        public event EventHandler OnStart;
        public event EventHandler<float> OnDownloadProgress;
        public event EventHandler<float> OnInstallProgress;
        public event EventHandler OnFinished;
        public event EventHandler<Exception> OnError;

        public async Task<GitHubRelease> Install(Plugin plugin)
        {
            try
            {
                // Trigger the event
                OnStart?.Invoke(this, EventArgs.Empty);

                // Retrieve the latest version from GitHub using the plugin's repository
                var latest = await GetLatestVersion(plugin);

                var temp = Path.Combine(Path.GetTempPath(), plugin.Name);
                Directory.CreateDirectory(temp);
                
                var installDir = new DirectoryInfo(plugin.DefaultInstallationPath);
                Directory.CreateDirectory(installDir.FullName);

                // For each asset, download it to the temp folder
                for (var index = 0; index < latest.Assets.Count; index++)
                {
                    var asset = latest.Assets[index];
                    var result = await _client.GetAsync(asset.BrowserDownloadUrl);
                    result.EnsureSuccessStatusCode();
                    var file = Path.Combine(temp, asset.Name);
                    
                    await using var fileStream = File.Create(file);
                    await result.Content.CopyToAsync(fileStream);
                    await fileStream.DisposeAsync();

                    // Extract the file if it's a zip
                    if (asset.Name.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ZipFile.ExtractToDirectory(file, installDir.FullName, true);
                    }

                    // Trigger the event
                    OnDownloadProgress?.Invoke(this, (float)index / (latest.Assets.Count));
                }

                // Move the files to the install directory
                var files = Directory.GetFiles(temp);
                for (var index = 0; index < files.Length; index++)
                {
                    var file = files[index];
                    if(file.EndsWith(".zip"))
                        continue;
                    
                    var dest = Path.Combine(installDir.FullName, Path.GetFileName(file));
                    File.Copy(file, dest, true);

                    // Trigger the event
                    OnInstallProgress?.Invoke(this, (float)index / (files.Length));
                }

                // Delete the temp directory
                Directory.Delete(temp, true);

                // Trigger the event
                OnFinished?.Invoke(this, EventArgs.Empty);
                
                plugin.IsInstalled = true;
                plugin.InstalledVersion = latest.TagName;
                        
                var profile = UserProfile.Get();
                var pluginIndex = profile.Plugins.IndexOf(profile.Plugins.First(x => x.Name == plugin.Name));
                profile.Plugins[pluginIndex] = plugin;
                UserProfile.Set(profile);

                return latest;
            } 
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to install plugin");
                
                // Trigger the event
                OnError?.Invoke(this, ex);

                throw;
            }
        }
    }
}