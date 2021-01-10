using System.ComponentModel;
using System.Windows.Controls;
using TagFinder.VersionManager;

namespace TagFinder.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Title { get; set; }

        public Page CurrentPage { get; set; }

        public bool IsOverlayVisible { get; set; }
        public string Status { get; set; }

        private readonly PageManager _pageManager;

        public MainViewModel(PageManager pageManager)
        {
            _pageManager = pageManager;
            _pageManager.PageChanged += (_, e) => CurrentPage = _pageManager.CurrentPage;

            StatusManager.StatusChanged += (_, e) => Status = StatusManager.Status;
            StatusManager.InProgressChanged += (_, e) => IsOverlayVisible = StatusManager.InProgress;

            Title = "Tag Finder " + Program.VersionManager.CurrentAppVersion.ToString();
        }
    }
}
