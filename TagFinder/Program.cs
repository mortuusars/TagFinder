using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using TagFinder.Infrastructure;
using TagFinder.InstagramAPI;
using TagFinder.Logger;
using TagFinder.Views.Pages;

namespace TagFinder
{
    public class Program
    {
        public const string APP_NAME = "TagFinder";
        public static readonly Version APP_VERSION = new Version("0.1.3");

        public static ViewManager ViewManager { get; private set; }
        public static PageManager PageManager { get; private set; }
        public static IInstagramAPI InstagramAPIService { get; private set; }

        public static Settings Settings { get; private set; }
        public static ILogger Logger { get; private set; }

        public void Startup()
        {
            CreateAppFolders();

            Logger = new FileLogger(FileNames.LOG_FILE);
            Settings = Settings.Load();

            if (Settings.CheckForUpdates)
                CheckUpdates();

            InstagramAPIService = new StandardInstagramAPI(FileNames.STATE_FILEPATH, Logger);

            PageManager = new PageManager();

            ViewManager = new ViewManager();
            ViewManager.ShowMainView();

            SetStartingPage();
        }

        private async void SetStartingPage()
        {
            if (!File.Exists(FileNames.LAST_USER_FILEPATH))
            {
                Logger.Log("No last user file");
                PageManager.SetPage(Pages.LoginPage);
                return;
            }

            var result = await InstagramAPIService.LogInAsync(File.ReadAllText(FileNames.LAST_USER_FILEPATH));

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
            VersionManager versionManager = new VersionManager(Logger);

            var (isUpdateAvailable, newVersion) = await versionManager.CheckNewVersion(APP_VERSION, FileNames.UPDATE_URL_FILE);

            if (isUpdateAvailable)
                ShowCanUpdateMessage(newVersion);
        }

        private static void ShowCanUpdateMessage(Version newVersion)
        {
            string message = "New update available. Open download page?\n" +
                                $"Current version: {APP_VERSION}\nAvailable version: {newVersion}";

            if (MessageBox.Show(message, "Tag Finder Update", MessageBoxButton.YesNo, MessageBoxImage.Information,
                MessageBoxResult.Yes) == MessageBoxResult.Yes)
            {
                string url = File.ReadAllText(FileNames.UPDATE_URL_FILE);

                var ps = new ProcessStartInfo(url)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);
            }
        }

        #endregion

        private void CreateAppFolders()
        {
            Directory.CreateDirectory(FileNames.CACHE);
            Directory.CreateDirectory(FileNames.APPLOCAL_FOLDER);
        }
    }
}
