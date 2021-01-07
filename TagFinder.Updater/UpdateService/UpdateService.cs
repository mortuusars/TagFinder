using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TagFinder.Updater
{
    public class UpdateService : IUpdateService
    {
        public event EventHandler<string> UpdateProcess;

        private const string UPDATER_FILENAME = "TagFinder.Updater";
        public string ZipFileName { get; private set; }

        public async Task<bool> Update()
        {
            string[] filesToDelete = await GetFilesToDeleteAsync();

            UpdateProcess?.Invoke(this, "Getting info from server");
            ZipFileName = $"TagFinder-{await GetVersionAsync()}.zip";

            UpdateProcess?.Invoke(this, $"Downloading {ZipFileName}");
            var (isSuccessful, errorReason) = await DownloadNewFileAsync(ZipFileName);

            if (!isSuccessful)
            {
                UpdateProcess?.Invoke(this, $"Downloading failed. {errorReason}");
                return false;
            }

            UpdateProcess?.Invoke(this, "Deleting old files");
            DeleteOldFiles(filesToDelete);

            UpdateProcess?.Invoke(this, "Unzipping downloaded file");
            UnzipArchive(ZipFileName);

            UpdateProcess?.Invoke(this, "Deleting temporary files");
            DeleteDownloadedFiles(new string[] { ZipFileName });

            UpdateProcess?.Invoke(this, "Finished");
            return true;
        }

        private async Task<string[]> GetFilesToDeleteAsync()
        {
            return await Task.Run(() =>
            {
                var files = Directory.GetFiles(Environment.CurrentDirectory);

                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = Path.GetFileName(files[i]);
                }

                return files.Where(f => !f.Contains(UPDATER_FILENAME)).ToArray();
            });
        }

        private void DeleteOldFiles(string[] filesToDelete)
        {
            foreach (var item in filesToDelete)
            {
                Debug.WriteLine("Deleting " + item + "...");
                File.Delete(item);
            }

            Debug.WriteLine("All Files Deleted");
        }

        private async Task<string> GetVersionAsync()
        {
            Debug.WriteLine("Getting version from server...");
            var request = await new HttpClient().GetAsync("http://mortuusars.000webhostapp.com/currentVersion.txt");

            var version = await request.Content.ReadAsStringAsync();
            Debug.WriteLine("Current version: " + version);

            return version;
        }

        private async Task<(bool isSuccessful, string errorReason)> DownloadNewFileAsync(string fileName)
        {
            Debug.WriteLine("Getting file from server...");
            var req = await new HttpClient().GetAsync($"http://mortuusars.000webhostapp.com/{fileName}");

            if (req.IsSuccessStatusCode)
            {
                using (Stream stream = await req.Content.ReadAsStreamAsync())
                {
                    Debug.WriteLine("Downloading file...");
                    using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                    {
                        long percent = stream.Length / 100;
                        Debug.WriteLine("");
                        while (stream.Position < stream.Length)
                        {
                            if (DateTime.Now.Millisecond % 10 == 0)
                            {
                                Debug.Write(Math.Floor((double)(stream.Position / percent)));
                            }

                            fs.WriteByte((byte)stream.ReadByte());
                        }
                    }
                }

                Debug.WriteLine("Downloaded");
                return (true, null);
            }
            else
            {
                Console.WriteLine("Getting file failed: " + req.ReasonPhrase);
                return (false, req.ReasonPhrase);
            }
        }

        private bool UnzipArchive(string fileName)
        {
            try
            {
                ZipFile.ExtractToDirectory(fileName, Environment.CurrentDirectory, overwriteFiles: false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void DeleteDownloadedFiles(string[] fileNames)
        {
            foreach (var file in fileNames)
            {
                File.Delete(file);
            }
        }
    }
}
