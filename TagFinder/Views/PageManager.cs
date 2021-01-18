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

        public Page CurrentPage { get; private set; }

        public void SetPage(Pages page)
        {
            Page newPage = null;

            switch (page)
            {
                case Pages.LoginPage:
                    newPage = new LoginPage() { DataContext = new LoginViewModel(Program.InstagramAPIService, Program.PageManager, Program.Logger) };
                    break;
                case Pages.TagsPage:
                    newPage = new TagsPage() { DataContext = new TagsViewModel(Program.InstagramAPIService, Program.PageManager, Program.ToastManager, Program.Logger) };
                    break;
            }

            CurrentPage = newPage;
            PageChanged?.Invoke(null, newPage);
        }
    }
}
