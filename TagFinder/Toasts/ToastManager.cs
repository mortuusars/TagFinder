using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TagFinder.Resources;

namespace TagFinder.Toasts
{
    public class ToastManager : IToastManager
    {
        public event EventHandler<Toast> ToastAdded;
        public ObservableCollection<Toast> ToastQueue { get; } = new ObservableCollection<Toast>();

        public void ShowMessage(string message, Icons icon = Icons.Info, bool isImportant = false)
        {
            ToastAdded?.Invoke(this, new Toast()
            {
                Message = message,
                Icon = icon,
                IsImportant = isImportant
            });
        }
    }
}
