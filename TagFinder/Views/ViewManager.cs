using TagFinder.ViewModels;
using TagFinder.Views;

namespace TagFinder
{
    public class ViewManager
    {
        #region MainView

        public static MainView MainView { get; set; }
        public static MainViewModel MainViewModel { get; set; }

        public static void ShowMainView()
        {
            if (MainView != null)
                return;

            MainViewModel = new MainViewModel();

            MainView = new MainView(Program.PageManager);
            MainView.DataContext = MainViewModel;

            MainView.Show();
        }

        public static void CloseMainView()
        {
            MainView?.Close();
        }

        #endregion

        #region Settings



        public static void ShowSettingsView()
        {

        }

        public static void CloseSettingsView()
        {

        }

        #endregion
    }
}
