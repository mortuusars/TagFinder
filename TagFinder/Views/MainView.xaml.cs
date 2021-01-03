using System.Windows;
using System.Windows.Controls;
using TagFinder.InstagramAPI;

namespace TagFinder
{
    public partial class MainView : Window
    {
        public MainView(PageManager pageManager)
        {
            InitializeComponent();

            pageManager.PageChanged += (_, page) => PageFrame.Content = page;
        }
    }
}
