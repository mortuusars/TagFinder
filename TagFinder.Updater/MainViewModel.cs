using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
            UpdateApp();
        }

        private void OnProcessUpdate(string status) => UpdateProcess = status;

        private async void UpdateApp()
        {
            InProgress = true;

            IUpdateService updateService = new UpdateService();
            updateService.UpdateProcess += (_, status) => OnProcessUpdate(status);
            bool isUpdateSuccessful = await updateService.Update();

            if (isUpdateSuccessful)
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

                SaveUpdateLog(updateService.UpdateLog);
            }
        }

        private void SaveUpdateLog(List<string> log)
        {
            StringBuilder sb = new StringBuilder(log.Count);

            foreach (var item in log)
            {
                sb.Append(item).Append('\n');
            }

            try
            {
                File.WriteAllText("updateLog.txt", sb.ToString());
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to save updatelog.");
            }
        }
    }
}
