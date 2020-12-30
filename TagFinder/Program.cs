using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagFinder.Views.Pages;

namespace TagFinder
{
    public class Program
    {
        public ViewManager ViewManager { get; private set; }

        public void Startup()
        {
            SetStartingPage();

            ViewManager = new ViewManager();
            ViewManager.ShowMainView();
        }

        private void SetStartingPage()
        {
            if (File.Exists(FileNames.LastUserFilePath))
            {
                var result = InstaApiService.TryLogInWithLastUser(File.ReadAllText(FileNames.LastUserFilePath));

                if (result == LogInRequiredActions.Success)
                {
                    StatusMessageService.ChangeStatusMessage("Logged as " + InstaApiService.LoggedUsername);
                    PageManager.SetPage(Pages.TagsPage);
                }
            }
            else
            {
                Logger.Log("No last user file");
                PageManager.SetPage(Pages.LoginPage);
            }
        }
    }
}
