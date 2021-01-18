using System;
using System.Collections.ObjectModel;
using TagFinder.Resources;

namespace TagFinder.Toasts
{
    public interface IToastManager
    {
        public event EventHandler<Toast> ToastAdded;

        public ObservableCollection<Toast> ToastQueue { get; }

        public void ShowMessage(string message, Icons icon = Icons.Info, bool isImportant = false);
    }
}
