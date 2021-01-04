using System.ComponentModel;

namespace TagFinder.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsOverlayVisible { get; set; }
        public string Status { get; set; }

        public MainViewModel()
        {
            StatusManager.StatusChanged += (_, e) => Status = StatusManager.Status;
            StatusManager.InProgressChanged += (_, e) => IsOverlayVisible = StatusManager.InProgress;
        }
    }
}
