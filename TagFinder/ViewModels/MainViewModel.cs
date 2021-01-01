using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using TagFinder.Views.Pages;
using Test;

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
