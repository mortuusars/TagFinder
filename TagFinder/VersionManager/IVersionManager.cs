using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFinder.VersionManager
{
    public interface IVersionManager
    {
        public Version CurrentAppVersion { get; }

        /// <summary>
        /// Checks version at the url and returns <see langword="true"/> if newer version is available.
        /// </summary>
        /// <param name="url">Url of the file.</param>
        /// <returns>Result of version comparison and info about new version.</returns>
        public Task<(bool isUpdateAvailable, VersionInfo versionInfo)> CheckNewVersionAsync(string url);
    }
}
