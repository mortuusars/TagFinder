using System;
using System.IO;
using System.Windows.Controls;
using TagFinder.ViewModels;
using TagFinder.Views.Pages;

namespace TagFinder
{
    public class PageManager
    {
        public event EventHandler<Page> PageChanged;

        public static Page CurrentPage { get; private set; }

        public void SetPage(Pages page)
        {
            Page newPage = null;

            switch (page)
            {
                case Pages.LoginPage:
                    newPage = new Login() { DataContext = new LoginViewModel() };
                    break;
                case Pages.TagsPage:
                    newPage = new TagsPage() { DataContext = new TagsViewModel() };
                    break;
            }

            CurrentPage = newPage;
            PageChanged?.Invoke(null, newPage);
        }

        private Page LoginPage()
        {
            var vm = new LoginViewModel();
            vm.LoggedIn += (s, _) => SetPage(Pages.TagsPage);

            var page = new Login() { DataContext = vm };
            page.
        }
    }
}
