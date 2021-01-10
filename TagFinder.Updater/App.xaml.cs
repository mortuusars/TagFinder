using System.Diagnostics;
using System.Windows;

namespace TagFinder.Updater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void OnTaskFinished()
        {
            var ps = new ProcessStartInfo("TagFinder.exe")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);

            App.Current.Shutdown();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string message = $"Something went wrong.\n{e.Exception.Message}\n{e.Exception.StackTrace}";

            MessageBox.Show(message, "TagFinder.Updater", MessageBoxButton.OK, MessageBoxImage.Error);

            Shutdown();
        }
    }
}
