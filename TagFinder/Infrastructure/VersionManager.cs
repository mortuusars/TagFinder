using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TagFinder.Logger;

namespace TagFinder.Infrastructure
{
    public class VersionManager
    {
        public string RecentChangelog { get; private set; }

        private readonly ILogger _logger;

        public VersionManager(ILogger logger)
        {
            this._logger = logger;
        }

        public async Task<bool> IsNewVersionAvailable(string versionFile, string urlFile)
        {
            string currentVerString = await GetVersionFromFile(versionFile);
            Version currentVersion = GetVersion(currentVerString);

            string webVerString = await GetWebVersionFromURL(urlFile);
            Version webVersion = GetVersion(webVerString);

            return CheckVersion(currentVersion, webVersion);
        }

        private static bool CheckVersion(Version currentVersion, Version webVersion)
        {
            if (webVersion == new Version() || currentVersion == new Version())
                return false;
            else if (webVersion > currentVersion)
                return true;
            else
                return false;
        }

        private Version GetVersion(string version)
        {
            try
            {
                _logger.Log($"Parsing {version} to version...");
                return Version.Parse(version);
            }
            catch (Exception ex)
            {
                _logger.Log("Cannot parse version: " + ex.Message);
                return new Version(0, 0, 0);
            }
        }

        private async Task<string> GetVersionFromFile(string versionFile)
        {
            _logger.Log("Getting version from file...");

            try
            {
                string ver = await File.ReadAllTextAsync(versionFile);
                _logger.Log("Current version: " + ver);
                return ver;
            }
            catch (Exception)
            {
                _logger.Log("Failed to read version from file");
                return string.Empty;
            }
        }

        private async Task<string> GetWebVersionFromURL(string urlFile)
        {
            _logger.Log("Getting version from url...");

            try
            {
                string url = await File.ReadAllTextAsync(urlFile);

                var client = new HttpClient();
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.Log("Got content from url: " + content);

                    content = content.Trim();

                    var verString = content.Substring(0, 4);

                    if (content.Length > 5)
                        RecentChangelog = content.Substring(4).Trim();

                    return verString;
                }
                else
                {
                    _logger.Log("Failed to retrieve version from url: " + response.StatusCode + response.ReasonPhrase);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.Log("Getting version from url failed: " + ex.Message);
            }

            return string.Empty;
        }
    }
}
