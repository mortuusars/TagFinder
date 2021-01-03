using System;
using System.IO;
using TagFinder.InstagramAPI;
using TagFinder.Logger;
using TagFinder.Views.Pages;

namespace TagFinder
{
    public class Program
    {
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

        private void CheckUpdates()
        {
            
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
