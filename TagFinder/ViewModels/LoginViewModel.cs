using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using TagFinder.InstagramAPI;
using TagFinder.Logger;

namespace TagFinder.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Username { get; set; }

        public bool IsCodeRequired { get; set; }
        public string CodeRequiredMessage { get; set; }

        public ICommand LogInCommand { get; }
        public ICommand SubmitCodeCommand { get; }

        private IInstagramAPI _instagramAPI;
        private PageManager _pageManager;
        private ILogger _logger;

        public LoginViewModel(IInstagramAPI instagramAPI, PageManager pageManager, ILogger logger)
        {
            _instagramAPI = instagramAPI;
            _pageManager = pageManager;
            _logger = logger;

            LogInCommand = new RelayCommand(pswrd => OnLogInCommand((string)pswrd));
            SubmitCodeCommand = new RelayCommand(code => OnSubmitCodeCommand((string)code));

            Username = RestoreLastUsername();
        }

        #region Last Username

        private string RestoreLastUsername()
        {
            try
            {
                if (File.Exists(FileNames.LAST_USER_FILEPATH))
                {
                    return File.ReadAllText(FileNames.LAST_USER_FILEPATH);
                }
            }
            catch (Exception)
            {
                _logger.Log("Failed restoring last username");
            }

            return string.Empty;
        }

        private void SaveLastUsername(string username)
        {
            try
            {
                File.WriteAllTextAsync(FileNames.LAST_USER_FILEPATH, username);
            }
            catch (Exception)
            {
                _logger.Log("Saving last username failed");
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

            StatusManager.Status = "Logging in as " + Username;
            StatusManager.InProgress = true;

            var logInResult = await _instagramAPI.LogInAsync(Username, password);

            switch (logInResult)
            {
                case LoginResult.Success:
                    StatusManager.Status = "Logged in as " + _instagramAPI.CurrentUserName;
                    _pageManager.SetPage(Views.Pages.Pages.TagsPage);
                    break;
                case LoginResult.PhoneNumberRequired:
                    StatusManager.Status = "Phone number required";
                    return;
                case LoginResult.SMSVerifyRequired:
                    IsCodeRequired = true;
                    CodeRequiredMessage = "Enter code from SMS:";
                    break;
                case LoginResult.EmailVerifyRequired:
                    IsCodeRequired = true;
                    CodeRequiredMessage = "Enter code from Email:";
                    break;
                case LoginResult.TwoFactorRequired:
                    StatusManager.Status = "Two factor verification required. Not suppoted";
                    return;
                case LoginResult.VerificationFailed:
                    StatusManager.Status = "Verification failed";
                    return;
                case LoginResult.Error:
                    StatusManager.Status = "Error";
                    break;
            }

            StatusManager.InProgress = false;
        }

        private async void OnSubmitCodeCommand(string code)
        {
            StatusManager.InProgress = true;
            StatusManager.Status = "Code submitted - Verifying";

            var codeVerifyResult = await _instagramAPI.ProvideVerificationCodeAsync(code);

            switch (codeVerifyResult)
            {
                case LoginResult.Success:
                    StatusManager.Status = "Logged in as " + _instagramAPI.CurrentUserName;
                    break;
                case LoginResult.TwoFactorRequired:
                    StatusManager.Status = "Two factor verification required. Not suppoted";
                    return;
                case LoginResult.Error:
                    StatusManager.Status = "Error";
                    return;
            }
            StatusManager.InProgress = false;
        }
    }
}
