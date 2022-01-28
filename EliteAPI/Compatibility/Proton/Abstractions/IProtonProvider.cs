using System.Collections.Generic;

namespace EliteAPI.Compatibility.Proton.Abstractions
{
    /// <summary>
    /// Provides functionality for proton specific game options.
    /// </summary>
    public interface IProtonProvider
    {
        /// <summary>
        ///     Gets the steam library locations
        /// </summary>
        /// <returns>Library locations</returns>
        IEnumerable<string> GetSteamLibraryLocations();
    }
}