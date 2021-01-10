using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TagFinder.Core.Logger;

namespace TagFinder.VersionManager
{
    public class JsonVersionManager : IVersionManager
    {
        public Version CurrentAppVersion { get; } = new Version(Program.APP_VERSION);

        private readonly ILogger _logger;

        public JsonVersionManager(ILogger logger)
        {
            this._logger = logger;

        #if DEBUG
            GenerateVersionJson();
        #endif
        }

        public async Task<(bool isUpdateAvailable, VersionInfo versionInfo)> CheckNewVersionAsync(string url)
        {
            VersionInfo webVersionInfo = await GetVersionInfoFromUrlAsync(url);

            if (webVersionInfo == null)
                return (false, null);

            Version webVersion = new Version(webVersionInfo.Version);

            bool isUpdateAvailable = CheckVersion(CurrentAppVersion, webVersion);

            return (isUpdateAvailable, webVersionInfo);
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
        /// Reads content from provided url as string.
        /// </summary>
        /// <returns><see langword="null"/> on error.</returns>
        private async Task<string> GetHttpStringAsync(string url)
        {
            _logger.Log($"Getting http string from [{url}]...");

            using (var client = new HttpClient())
            {
                HttpResponseMessage response;
                try
                {
                    response = await client.GetAsync(url);
                }
                catch (Exception ex)
                {
                    _logger.Log("Failed to load url:\n" + ex.Message);
                    return null;
                }

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
                else
                {
                    _logger.Log($"Http response was not successful:\n{response.StatusCode}\n{response.ReasonPhrase}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Load latestVersion file.
        /// </summary>
        private async Task<VersionInfo> GetVersionInfoFromUrlAsync(string url)
        {
            string jsonString = await GetHttpStringAsync(url);

            if (jsonString == null)
                return null;

            VersionInfo versionInfo = JsonSerializer.Deserialize<VersionInfo>(jsonString);
            return versionInfo;
        }

        /// <summary>
        /// Generates file with version and latest changes for uploading to FTP server.
        /// </summary>
        private void GenerateVersionJson()
        {
            string changelog;
            try
            {
                changelog = File.ReadAllText(FilePath.LATEST_CHANGELOG_FILE);
            }
            catch (Exception)
            {
                _logger.Log($"[{typeof(JsonVersionManager)}] - Cannot read latest changelog file.\n" +
                    $"Generated version .json will not contain proper changelog information.");
                changelog = "Could not read changelog file.";
            }

            var info = new VersionInfo()
            {
                Version = CurrentAppVersion.ToString(),
                Changelog = changelog
            };

            string jsonString = JsonSerializer.Serialize(info, new JsonSerializerOptions() { WriteIndented = true });

            try
            {
                File.WriteAllText("latestVersion.json", jsonString);
            }
            catch (Exception ex)
            {
                _logger.Log($"[{typeof(JsonVersionManager)}] - Cannot write to latestVersion.json:\n{ex.Message}");
            }
        }
    }
}
