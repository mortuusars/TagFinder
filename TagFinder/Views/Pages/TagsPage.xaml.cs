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

            UsernameBox.Text = "mortuus_cg";
        }

        private void AddTagPlusButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTagsButtonsPanel.Visibility = Visibility.Collapsed;

            AddTagByNamePanel.Visibility = Visibility.Visible;
            AddTagTextBox.Focus();
        }

        private async void ClearAndHideAddingTagByName(object sender, RoutedEventArgs e)
        {
            await Task.Delay(20);

            AddTagTextBox.Text = string.Empty;
            AddTagByNamePanel.Visibility = Visibility.Collapsed;

            SelectedTagsButtonsPanel.Visibility = Visibility.Visible;
        }

        private void TextBoxNumberChange_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var box = (TextBox)sender;

            int currentNumber;
            try
            {
                currentNumber = Convert.ToInt32(box.Text);
            }
            catch (Exception)
            {
                currentNumber = 2;
            }

            if (e.Delta > 0)
            {
                currentNumber++;
            }
            else
            {
                if (currentNumber <= 1)
                    return;

                currentNumber--;
            }

            box.Text = currentNumber.ToString();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var context = ((Button)sender).DataContext as TagRecord;

                AllTagsListBox.SelectedItems.Add(context);

                await Task.Delay(10);

                AllTagsListBox.SelectedItems.Clear();
        }
    }
}
