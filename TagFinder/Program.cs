using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using TagFinder.Infrastructure;
using TagFinder.Core.InstagramAPI;
using TagFinder.Core.Logger;
using TagFinder.Views.Pages;
using System.Text.Json;
using TagFinder.VersionManager;

namespace TagFinder
{
    public class Program
    {
        public const string APP_NAME = "TagFinder";
        public static readonly Version APP_VERSION = new Version("0.2.0");

        //public static ViewManager ViewManager { get; private set; }
        public static PageManager PageManager { get; private set; }
        public static IInstagramAPI InstagramAPIService { get; private set; }

        public static Settings Settings { get; private set; }
        public static ILogger Logger { get; private set; }

        public void Startup()
        {
            CreateAppFolders();

            Logger = new FileLogger(FilePath.LOG_FILE);
            Settings = Settings.Load();

            if (Settings.CheckForUpdates)
                CheckUpdates();

            InstagramAPIService = new InstagramAPI(FilePath.STATE_FILEPATH, Logger);

            PageManager = new PageManager();

            //ViewManager = new ViewManager();
            ViewManager.ShowMainView();

            SetStartingPage();

#if DEBUG
            GenerateVersionJson();
#endif
        }

        private static void GenerateVersionJson()
        {
            var info = new VersionInfo()
            {
                Version = APP_VERSION.ToString(),
                Changelog = File.ReadAllText(FilePath.LATEST_CHANGELOG_FILE)
            };

            string jsonString = JsonSerializer.Serialize(info, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText("latestVersion.json", jsonString);
        }

        private async void SetStartingPage()
        {
            if (!File.Exists(FilePath.LAST_USER_FILEPATH))
            {
                Logger.Log("No last user file");
                PageManager.SetPage(Pages.LoginPage);
                return;
            }

            var result = await InstagramAPIService.LogInAsync(File.ReadAllText(FilePath.LAST_USER_FILEPATH));

            if (result == LoginResult.Success)
            {
                StatusManager.Status = "Loading user info";
                StatusManager.InProgress = true;

                await InstagramAPIService.DownloadUserProfilePicAsync(InstagramAPIService.CurrentUserName);
                StatusManager.Status = "Logged as " + InstagramAPIService.CurrentUserName;
                PageManager.SetPage(Pages.TagsPage);
            }
            else
            {
                PageManager.SetPage(Pages.LoginPage);
            }

            StatusManager.InProgress = false;
        }

        #region Update

        private async void CheckUpdates()
        {
            IVersionManager versionManager = new JsonVersionManager(Logger);

            var (isUpdateAvailable, versionInfo) = await versionManager.CheckNewVersionAsync(APP_VERSION, File.ReadAllText(FilePath.UPDATE_URL_FILE));

            if (!isUpdateAvailable)
                return;

            if (AskUserForUpdate(versionInfo))
            {
                //if (versionInfo.ShouldUpdateManually)
                //    StartUpdateProcess(FilePath.MANUAL_UPDATE_URL_FILE);
                //else
                //    StartUpdateProcess("TagFinder.Updater.exe");

                //App.Current.Shutdown();
            }
        }

        private void StartUpdateProcess(string name)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(name)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(processStartInfo);
        }

        private static bool AskUserForUpdate(VersionInfo newVersionInfo)
        {
            string changelog = newVersionInfo.Changelog;

            if (changelog.Length > 300)
                changelog = changelog.Substring(0, 300) + "...\nAnd many more changes!";

            string updateAction = "";

            if (newVersionInfo.ShouldUpdateManually)
                updateAction = "Manual update needed. Open download page?";
            else
                updateAction = "Update automatically?";

            string message = $"New update available. {updateAction}\n\n" +
                             $"Current version: {APP_VERSION}\n" +
                             $"New version: {newVersionInfo.Version}" +
                             $"\n\nChanges:\n\n{changelog}";

            if (MessageBox.Show(message, "Tag Finder Update", MessageBoxButton.YesNo, MessageBoxImage.Information,
                MessageBoxResult.Yes) == MessageBoxResult.Yes)
                return true;

            return false;
        }

        #endregion

        private void CreateAppFolders()
        {
            Directory.CreateDirectory(FilePath.CACHE);
            Directory.CreateDirectory(FilePath.APPLOCAL_FOLDER);
        }
    }
}
