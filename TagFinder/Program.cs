﻿using System;
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
        public static readonly Version APP_VERSION = new Version(0,1,0);

        public static ViewManager ViewManager { get; private set; }
        public static PageManager PageManager { get; private set; }
        public static IInstagramAPI InstagramAPIService { get; private set; }

        public static ILogger Logger { get; } = new FileLogger(FileNames.LOG_FILE);

        public void Startup()
        {
            CreateAppFolders();

            CheckUpdates();

            InstagramAPIService = new StandardInstagramAPI(FileNames.STATE_FILEPATH, Logger);

            PageManager = new PageManager();

            ViewManager = new ViewManager();
            ViewManager.ShowMainView();

            SetStartingPage();
        }

        private async void CheckUpdates()
        {
            VersionManager versionManager = new VersionManager(Logger);
            var updateAvailable = await versionManager.IsNewVersionAvailable(APP_VERSION, FileNames.UPDATE_URL_FILE);

            if (updateAvailable)
            {
                if (MessageBox.Show("New update available. Open download page?\n\n" + versionManager.RecentChangelog, "Tag Finder Update", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {

                    if (File.Exists(FileNames.DOWNLOAD_URL_FILE))
                    {
                        string url = File.ReadAllText(FileNames.DOWNLOAD_URL_FILE);

                        var ps = new ProcessStartInfo(url)
                        {
                            UseShellExecute = true,
                            Verb = "open"
                        };
                        Process.Start(ps);
                    }

                }
            }
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

        private void CreateAppFolders()
        {
            Directory.CreateDirectory(FileNames.CACHE_FOLDER);
            Directory.CreateDirectory(FileNames.LOCAL_FOLDER + "/TagFinder/");
        }
    }
}
