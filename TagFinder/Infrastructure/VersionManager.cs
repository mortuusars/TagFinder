using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TagFinder.Logger;

namespace TagFinder.Infrastructure
{
    public class VersionManager
    {
        public VersionInfo WebVersionInfo { get; private set; }

        private readonly ILogger _logger;

        public VersionManager(ILogger logger)
        {
            this._logger = logger;
        }

        public void GenerateVersionJson()
        {
            VersionInfo vi = new VersionInfo() { CurrentVersion = Program.APP_VERSION, ChangeLog = "#Added:\n-Checking for updates" };

            File.WriteAllText(FileNames.CURRENT_VERSION_FILE, JsonSerializer.Serialize(vi, new JsonSerializerOptions() { WriteIndented = true }));
        }

        /// <summary>
        /// Checks version on the GitHub and returns <see langword="true"/> if newer version is available.
        /// </summary>
        /// <param name="currentVersion">Current version of the program.</param>
        /// <param name="url">Url of the github file.</param>
        /// <returns>Result of version comparison and info about new version.</returns>
        public async Task<(bool isUpdateAvailable, VersionInfo info)> CheckNewVersion(Version currentVersion, string url)
        {
            VersionInfo webVersion = await GetWebVersionInfoFromURL(url);

            return (CheckVersion(currentVersion, webVersion.CurrentVersion), webVersion);
        }

        /// <summary>
        /// Compare current version with version on the web.
        /// </summary>
        private static bool CheckVersion(Version currentVersion, Version webVersion)
        {
            if (webVersion == new Version() || currentVersion == new Version())
                return false;
            else if (webVersion > currentVersion)
                return true;
            else
                return false;
        }

        //private Version GetVersion(string version)
        //{
        //    try
        //    {
        //        _logger.Log($"Parsing {version} to version...");
        //        return Version.Parse(version);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Log("Cannot parse version: " + ex.Message);
        //        return new Version(0, 0, 0);
        //    }
        //}

        //private async Task<string> GetVersionFromFile(string versionFile)
        //{
        //    _logger.Log("Getting version from file...");

        //    try
        //    {
        //        string ver = await File.ReadAllTextAsync(versionFile);
        //        ver = ver.Trim().Substring(0, 5);
        //        _logger.Log("Current version: " + ver);
        //        return ver;
        //    }
        //    catch (Exception)
        //    {
        //        _logger.Log("Failed to read version from file");
        //        return string.Empty;
        //    }
        //}

        private async Task<VersionInfo> GetWebVersionInfoFromURL(string url)
        {
            _logger.Log("Getting version from web url...");

            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<VersionInfo>(content);
                }
                else
                {
                    _logger.Log("Failed to retrieve version from url: " + response.StatusCode + " " + response.ReasonPhrase);
                    return new VersionInfo();
                }
            }
            catch (Exception ex)
            {
                _logger.Log("Getting version from url failed: " + ex.Message);
            }

            return new VersionInfo();
        }
    }
}
