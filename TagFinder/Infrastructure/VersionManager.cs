using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TagFinder.Core.Logger;

namespace TagFinder.Infrastructure
{
    public class VersionManager
    {
        private readonly ILogger _logger;

        public VersionManager(ILogger logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// Checks version on the GitHub and returns <see langword="true"/> if newer version is available.
        /// </summary>
        /// <param name="currentVersion">Current version of the program.</param>
        /// <param name="url">Url of the github file.</param>
        /// <returns>Result of version comparison and info about new version.</returns>
        public async Task<(bool isUpdateAvailable, Version newVersion)> CheckNewVersion(Version currentVersion, string url)
        {
            Version webVersion = await GetWebVersionFromURL(File.ReadAllText(url));

            return (CheckVersion(currentVersion, webVersion), webVersion);
        }

        /// <summary>
        /// Compare current version with version on the web.
        /// </summary>
        private bool CheckVersion(Version currentVersion, Version webVersion)
        {
            if (webVersion == new Version() || currentVersion == new Version())
                return false;
            else if (webVersion > currentVersion)
            {
                _logger.Log("New version available: " + webVersion.ToString());
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Load latest releases page, find version tag, extract version.
        /// </summary>
        private async Task<Version> GetWebVersionFromURL(string url)
        {
            _logger.Log("Getting version from web url...");

            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    string versionString = "";
                    try
                    {
                        string link = Regex.Match(content, "/mortuusars/TagFinder/releases/tag/\\d+\\.\\d+\\.\\d+").ToString();
                        versionString = Regex.Match(link, "\\d+\\.\\d+\\.\\d+").ToString();
                    }
                    catch (Exception ex)
                    {
                        _logger.Log("Parsing github content failed: " + ex.Message);
                    }

                    return Version.TryParse(versionString, out Version result) ? result : new Version();
                }
                else
                {
                    _logger.Log("Failed to retrieve version from url: " + response.StatusCode + " " + response.ReasonPhrase);
                    return new Version();
                }
            }
            catch (Exception ex)
            {
                _logger.Log("Getting version from url failed: " + ex.Message);
            }

            return new Version();
        }
    }
}
