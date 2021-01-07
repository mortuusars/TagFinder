using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TagFinder.Core.Logger;

namespace TagFinder.VersionManager
{
    public class JsonVersionManager : IVersionManager
    {
        private readonly ILogger _logger;

        public JsonVersionManager(ILogger logger)
        {
            this._logger = logger;
        }

        public async Task<(bool isUpdateAvailable, VersionInfo versionInfo)> CheckNewVersionAsync(Version currentVersion, string url)
        {
            VersionInfo webVersionInfo = await GetVersionJsonFromUrlAsync(url);

            if (webVersionInfo == null)
                return (false, null);

            Version webVersion = new Version(webVersionInfo.Version);
            return (CheckVersion(currentVersion, webVersion), webVersionInfo);
        }

        /// <summary>
        /// Compare current version with version on the web.
        /// </summary>
        private bool CheckVersion(Version currentVersion, Version webVersion)
        {
            if (webVersion == currentVersion)
            {
                _logger.Log("You are on the latest version:");
                _logger.Log("Current version: " + currentVersion.ToString());
                _logger.Log("Web version: " + webVersion.ToString());
                return false;
            }
            else if (webVersion > currentVersion)
            {
                _logger.Log("Version on the web is higher than current:");
                _logger.Log("Current version: " + currentVersion.ToString());
                _logger.Log("Web version: " + webVersion.ToString());
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Load latestVersion file.
        /// </summary>
        private async Task<VersionInfo> GetVersionJsonFromUrlAsync(string url)
        {
            _logger.Log("Getting version json from web url...");

            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var info = JsonSerializer.Deserialize<VersionInfo>(jsonString);
                    return info;
                }
                else
                {
                    _logger.Log("Failed to retrieve version from url:\n" + response.StatusCode + "\n" + response.ReasonPhrase);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Log("Getting version from url failed:\n" + ex.Message);
            }

            return null;
        }
    }
}
