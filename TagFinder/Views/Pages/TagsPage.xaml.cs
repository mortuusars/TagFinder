using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TagFinder.Views.Pages
{
    /// <summary>
    /// Interaction logic for TagsPage.xaml
    /// </summary>
    public partial class TagsPage : Page
    {
        public TagsPage()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (AllTagsListBox.SelectedItem == null)
                return;

            //((TagRecord)AllTagsListBox.SelectedItem).GetGlobalCountString();

            //MessageBox.Show(result);
        }
    }
}
