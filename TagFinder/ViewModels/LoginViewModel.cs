using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TagFinder.Infrastructure;
using TagFinder.Views.Pages;

namespace TagFinder.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler LoggedIn;
        public event EventHandler LoggedOut;

        public string Username { get; set; }

        public bool IsCodeRequired { get; set; }
        public string CodeRequiredMessage { get; set; }

        public ICommand LogInCommand { get; }
        public ICommand SubmitCodeCommand { get; }

        public LoginViewModel()
        {
            LogInCommand = new RelayCommand(pswrd => OnLogInCommand((string)pswrd));
            SubmitCodeCommand = new RelayCommand(code => OnSubmitCodeCommand((string)code));

            Username = RestoreLastUsername();
        }

        #region Last Username

        private string RestoreLastUsername()
        {
            try
            {
                if (File.Exists(FileNames.LastUserFilePath))
                {
                    return File.ReadAllText(FileNames.LastUserFilePath);
                }
            }
            catch (Exception)
            {
                Logger.Log("Failed restoring last username");
            }

            return string.Empty;
        }

        private void SaveLastUsername(string username)
        {
            try
            {
                File.WriteAllTextAsync(FileNames.LastUserFilePath, username);
            }
            catch (Exception)
            {
                Logger.Log("Saving last username failed");
            }
        }

        #endregion

        private async void OnLogInCommand(string password)
        {
            SaveLastUsername(Username);

            if (password.Length == 0)
            {
                MessageBox.Show("You must enter a password to log in");
                return;
            }

            ViewManager.MainViewModel.IsOverlayVisible = true;
            StatusMessageService.ChangeStatusMessage("Logging in as " + Username);

            var logInResult = await InstaApiService.LogIn(Username, password);

            switch (logInResult)
            {
                case LogInRequiredActions.Success:
                    StatusMessageService.ChangeStatusMessage("Successfully logged in");
                    LoggedIn?.Invoke(this, EventArgs.Empty);
                    break;
                case LogInRequiredActions.PhoneNumberRequired:
                    StatusMessageService.ChangeStatusMessage("Phone number required");
                    return;
                case LogInRequiredActions.SMSVerifyRequired:
                    IsCodeRequired = true;
                    CodeRequiredMessage = "Enter code from SMS:";
                    break;
                case LogInRequiredActions.EmailVerifyRequired:
                    IsCodeRequired = true;
                    CodeRequiredMessage = "Enter code from Email:";
                    break;
                case LogInRequiredActions.TwoFactorRequired:
                    StatusMessageService.ChangeStatusMessage("Two factor verification required. Not suppoted");
                    return;
                case LogInRequiredActions.VerificationFailed:
                    StatusMessageService.ChangeStatusMessage("Verification failed");
                    return;
                case LogInRequiredActions.Error:
                    StatusMessageService.ChangeStatusMessage("Error");
                    return;
            }

            ViewManager.MainViewModel.IsOverlayVisible = false;
            StatusMessageService.ChangeStatusMessage("");
        }

        private async void OnSubmitCodeCommand(string code)
        {
            ViewManager.MainViewModel.IsOverlayVisible = true;
            StatusMessageService.ChangeStatusMessage("Code submitted\nVerifying");

            var codeVerifyResult = await InstaApiService.ProvideVerificationCode(code);

            switch (codeVerifyResult)
            {
                case LogInRequiredActions.Success:
                    StatusMessageService.ChangeStatusMessage("Successfully logged in");
                    LoggedIn?.Invoke(this, EventArgs.Empty);
                    break;
                case LogInRequiredActions.TwoFactorRequired:
                    StatusMessageService.ChangeStatusMessage("Two factor verification required. Not suppoted");
                    return;
                case LogInRequiredActions.Error:
                    StatusMessageService.ChangeStatusMessage("Error");
                    return;
            }

            ViewManager.MainViewModel.IsOverlayVisible = false;
            StatusMessageService.ChangeStatusMessage("");
        }
    }
}
