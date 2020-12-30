using TagFinder.ViewModels;
using TagFinder.Views;

namespace TagFinder
{
    public class ViewManager
    {
        public static MainView MainView { get; set; }
        public static MainViewModel MainViewModel { get; set; }

        public void ShowMainView()
        {
            MainViewModel = new MainViewModel();

            MainView = new MainView();
            MainView.DataContext = MainViewModel;

            MainView.Show();
        }

        public void CloseMainView()
        {
            MainView.Close();
        }
    }
}
