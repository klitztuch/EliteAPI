using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EliteAPI.Compatibility.Proton.Abstractions;

namespace EliteAPI.Compatibility.Proton
{
    /// <inheritdoc />
    public class ProtonProvider : IProtonProvider
    {
        /// <inheritdoc />
        public IEnumerable<string> GetSteamLibraryLocations()
        {
            var pattern = "(\"path\")\\W*\"(.*)\"";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var configLines = File.ReadAllLines(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".steam/steam/config/libraryfolders.vdf"));
            var paths = configLines.Select(o => regex.Match(o))
                .Where(o => o.Success)
                .Select(o => o.Groups[2].Value);
            return paths;
        }
    }
}