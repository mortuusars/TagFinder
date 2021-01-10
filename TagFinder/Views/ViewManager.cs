using TagFinder.ViewModels;
using TagFinder.Views;

namespace TagFinder
{
    public static class ViewManager
    {
        #region MainView

        public static MainView MainView { get; set; }
        public static MainViewModel MainViewModel { get; set; }

        public static void ShowMainView()
        {
            if (MainView != null)
                return;

            MainViewModel = new MainViewModel(Program.PageManager);

            MainView = new MainView();
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
