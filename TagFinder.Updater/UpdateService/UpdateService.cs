using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace TagFinder.Updater
{
    public class UpdateService : IUpdateService
    {
        public event EventHandler<string> UpdateProcess;

        /// <summary>
        /// Log with additional information about update process.
        /// </summary>
        public List<string> UpdateLog { get; private set; } = new List<string>();
        public string ZipFileName { get; private set; }

        private const int HTTP_TIMEOUT_SECONDS = 30;

        /// <summary>
        /// Initializes update process.
        /// </summary>
        /// <returns><see langword="true"/> if updated successfully.</returns>
        public async Task<bool> Update()
        {
            UpdateProcess?.Invoke(this, "Getting info from server");

            string url = GetUrlFromFile("updateUrl.txt");
            if (url == null)
                return false;

            UpdateProcess?.Invoke(this, $"Downloading {ZipFileName}");

            string version = await GetVersionStringAsync(url);
            if (version == null)
                return false;

            ZipFileName = $"TagFinder-{version}.zip";
            string downloadUrl = GetUrlFromFile("downloadUrl.txt");
            var (isSuccessful, errorReason) = await DownloadArchiveAsync(downloadUrl, ZipFileName);

            if (!isSuccessful)
            {
                UpdateProcess?.Invoke(this, $"Downloading failed.\n{errorReason}");
                return false;
            }

            UpdateProcess?.Invoke(this, "Unzipping downloaded file");
            if (!UnzipArchive(ZipFileName))
                return false;

            UpdateProcess?.Invoke(this, "Deleting temporary files");
            DeleteDownloadedFiles(new string[] { ZipFileName });

            UpdateProcess?.Invoke(this, "Finished");
            return true;
        }

        private async Task<string> ReadUrlAsStringAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("Url cannot be null or empty");

            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            HttpResponseMessage response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                UpdateLog.Add($"Response was unsuccessful:\n{response.StatusCode}\n{response.ReasonPhrase}");
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        private string GetUrlFromFile(string fileName)
        {
            try
            {
                UpdateLog.Add($"Reading file: [{fileName}]...");
                return File.ReadAllText(fileName);
            }
            catch (Exception ex)
            {
                UpdateLog.Add("Failed to read url file:\n" + ex.Message);
                return null;
            }
        }

        private async Task<string> GetVersionStringAsync(string url)
        {
            UpdateLog.Add($"Getting version from [{url}]...");
            string json = await ReadUrlAsStringAsync(url);

            try
            {
                VersionInfo versionInfo = JsonSerializer.Deserialize<VersionInfo>(json);
                UpdateLog.Add("Web Version is: " + versionInfo.Version);
                return versionInfo.Version;
            }
            catch (Exception ex)
            {
                UpdateLog.Add("Failed to deserialize version.\n" + ex.Message);
                return null;
            }
        }

        //private async Task<string[]> GetDirectoryFiles()
        //{
        //    return await Task.Run(() =>
        //    {
        //        var files = Directory.GetFiles(Environment.CurrentDirectory);

        //        for (int i = 0; i < files.Length; i++)
        //        {
        //            files[i] = Path.GetFileName(files[i]);
        //        }

        //        return files;
        //    });
        //}

        //private void DeleteOldFiles(string[] filesToDelete)
        //{
        //    foreach (var item in filesToDelete)
        //    {
        //        Debug.WriteLine("Deleting " + item + "...");
        //        try
        //        {
        //            Debug.WriteLine(item);
        //            //File.Delete(item);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(item + ex.Message);
        //        }
        //    }

        //    Debug.WriteLine("All Files Deleted");
        //}


        private async Task<(bool isSuccessful, string errorReason)> DownloadArchiveAsync(string downloadUrl, string fileName)
        {
            UpdateLog.Add($"\nGetting [{fileName}] from [{downloadUrl}]...");

            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS);

            try
            {
                HttpResponseMessage response = await client.GetAsync(downloadUrl + fileName);

                if (!response.IsSuccessStatusCode)
                {
                    UpdateLog.Add($"Response was unsuccessful:\n{response.StatusCode}\n{response.ReasonPhrase}");
                    return (false, response.ReasonPhrase);
                }

                using (Stream stream = await response.Content.ReadAsStreamAsync())
                {
                    UpdateLog.Add("Downloading file...");
                    using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                    {
                        while (stream.Position < stream.Length)
                        {
                            fs.WriteByte((byte)stream.ReadByte());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateLog.Add($"Failed to download archive:\n{ex.Message}");
                return (false, ex.Message);
            }

            UpdateLog.Add("Archive downloaded.");
            return (true, null);
        }

        private bool UnzipArchive(string archiveFileName)
        {
            List<string> extractedFiles = new List<string>();
            List<string> errorFiles = new List<string>();

            UpdateLog.Add("\nExtracting from archive...");

            try
            {
                using (ZipArchive zipArchive = ZipFile.OpenRead(archiveFileName))
                {
                    foreach (ZipArchiveEntry file in zipArchive.Entries)
                    {
                        try
                        {
                            file.ExtractToFile(file.Name, true);
                            extractedFiles.Add(file.Name);
                        }
                        catch (Exception)
                        {
                            errorFiles.Add(file.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateLog.Add("Unzipping archive failed:\n" + ex.Message);
                return false;
            }

            string extracted = "";
            extractedFiles.ForEach(item => extracted += $"{item}\n");

            string errored = "";
            errorFiles.ForEach(item => errored += $"{item}\n");

            UpdateLog.Add($"Extracted files:\n{extracted}\nErrored:\n{errored}");

            return true;
        }

        private void DeleteDownloadedFiles(string[] fileNames)
        {
            UpdateLog.Add("Deleting temporary files...");
            
            string deletedFiles = "";

            foreach (var file in fileNames)
            {
                try
                {
                    File.Delete(file);
                    deletedFiles += file + "\n";
                }
                catch (Exception ex)
                {
                    UpdateLog.Add($"[TEMPDELETE] Failed to delete [{file}]\n" + ex.Message);
                }
            }

            UpdateLog.Add("\nDeleted files:\n" + deletedFiles);

        }
    }
}
