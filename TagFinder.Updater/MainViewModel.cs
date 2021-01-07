using System;
using System.Windows.Input;
using System.Windows.Threading;
using PropertyChanged;

namespace TagFinder.Updater
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel
    {
        public string UpdateProcess { get; private set; }
        public bool InProgress { get; set; }

        public MainViewModel()
        {
            IUpdateService updateService = new UpdateService();
            updateService.UpdateProcess += (_, status) => OnProcessUpdate(status);
            updateService.Update();
        }

        private void OnProcessUpdate(string status)
        {
            if (status.Contains("Failed") || status.Contains("Finished"))
            {
                InProgress = false;

                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    App.OnTaskFinished();
                };

                timer.Start();
            }

            UpdateProcess = status;
        }
    }
}
