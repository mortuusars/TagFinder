using System.Windows;
using System.Windows.Threading;

namespace TagFinder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            new Program().Startup();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Program.Logger.Log("EXCEPTION: " + e.Exception.Message + "\n" + e.Exception.StackTrace);
            MessageBox.Show("Tag Finder encountered an exception: " + e.Exception.Message + "\n Program will exit.", "Tag Finder",
                MessageBoxButton.OK, MessageBoxImage.Error);

            this.Shutdown();
        }
    }
}
