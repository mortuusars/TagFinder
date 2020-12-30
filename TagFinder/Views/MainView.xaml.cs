using System.Windows;
using System.Windows.Controls;

namespace TagFinder
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            PageFrame.Content = PageManager.GetMainViewPage();

            PageManager.PageChanged += OnPageChanged;
        }

        private void OnPageChanged(object sender, Page e)
        {
            PageFrame.Content = e;
        }
    }
}
