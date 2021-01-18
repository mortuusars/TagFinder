using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using TagFinder.Toasts;

namespace TagFinder.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ToastControlViewModel
    {
        const int ShowingTimeMS = 2000;
        const int AnimationDelayMS = 300;

        public Toast CurrentToast { get; set; } = new Toast() { Message = "" };
        public bool IsVisible { get; set; }

        public bool IsToastsEnabled { get; set; }

        readonly List<Toast> toastQueue = new List<Toast>();

        readonly IToastManager _toastManager;

        public ToastControlViewModel()
        {
            _toastManager = Program.ToastManager;
            _toastManager.ToastAdded += OnToastAdded;

            IsToastsEnabled = true;

            StartShowingToasts();
        }

        private void OnToastAdded(object sender, Toast toast)
        {
            toastQueue.Add(toast);
        }

        private async void StartShowingToasts()
        {
            while (IsToastsEnabled)
            {
                while (toastQueue.Count > 0)
                {
                    IsVisible = false;

                    if (toastQueue.Count > 1)
                    {
                        await Task.Delay(AnimationDelayMS);
                    }

                    CurrentToast = toastQueue[0];
                    IsVisible = true;

                    int showingTime = Math.Max(ShowingTimeMS / 6, ShowingTimeMS / toastQueue.Count);
                    showingTime = CurrentToast.IsImportant ? showingTime + ShowingTimeMS / 4 : showingTime;

                    await Task.Delay(showingTime);
                    IsVisible = false;
                    toastQueue.Remove(CurrentToast);
                }

                await Task.Delay(200);
            }
        }
    }
}
