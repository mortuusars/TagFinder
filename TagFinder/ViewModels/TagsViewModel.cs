using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TagFinder.Infrastructure;

namespace TagFinder.ViewModels
{
    public class TagsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private const string PREF_DIRECTORY = "Cache/";
        private const string PAGES_FILENAME = "pagesToLoad.";
        private const string TAGLIMIT_FILENAME = "tagLimit.";

        public List<TagRecord> TagsList { get; set; }
        public ObservableCollection<TagRecord> SelectedTagsList { get; set; } = new ObservableCollection<TagRecord>();

        public bool IsContentAvailable { get; set; }

        public int PagesToLoad { get; set; } = 5;
        public int TagLimit { get; set; } = 20;
        public bool IncludeGlobalCount { get; set; } = false;

        public string Status { get; set; } = StatusMessageService.Status;
        public string SelectedCount { get; set; }

        public bool IsUserInfoAvailable { get; set; }
        public string LoggedUsername { get; set; }
        public BitmapImage UserProfilePic { get; set; } = Utility.GetDefaultProfilePic();



        public ICommand GetTagsCommand { get; }
        public ICommand ClearSelectedCommand { get; }
        public ICommand RemoveSelectedCommand { get; }
        public ICommand AddToSelectedCommand { get; }
        public ICommand CopySelectedCommand { get; }
        public ICommand LogOutCommand { get; }

        public TagsViewModel()
        {
            GetTagsCommand = new RelayCommand(username => OnGetTagsCommand((string)username));
            ClearSelectedCommand = new RelayCommand(_ => SelectedTagsList.Clear());
            RemoveSelectedCommand = new RelayCommand(items => RemoveSelectedItems((System.Collections.IList)items));
            AddToSelectedCommand = new RelayCommand(items => AddToSelected((System.Collections.IList)items));
            CopySelectedCommand = new RelayCommand(_ => CopyToClipboard());
            LogOutCommand = new RelayCommand(_ => OnLogOutCommand());
            
            StatusMessageService.StatusChanged += (_, message) => Status = message;

            SelectedTagsList.CollectionChanged += OnSelectedChanged;

            SetUserProfileInfoAsync();

            ReadPreferences();
        }

        private async void OnLogOutCommand()
        {
            ViewManager.MainViewModel.IsOverlayVisible = true;
            StatusMessageService.ChangeStatusMessage("Logging out");
            await InstaApiService.LogOut();
            PageManager.SetPage(Views.Pages.Pages.LoginPage);

            ViewManager.MainViewModel.IsOverlayVisible = false;
            StatusMessageService.ChangeStatusMessage("Logged out");
        }

        private async void SetUserProfileInfoAsync()
        {
            UserProfilePic = await InstaApiService.DownloadUserProfilePic(InstaApiService.LoggedUsername);
            LoggedUsername = InstaApiService.LoggedUsername;
            IsUserInfoAvailable = true;
        }

        private void OnSelectedChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SelectedCount = SelectedTagsList.Count > 0 ? SelectedTagsList.Count.ToString() : "";
        }

        private void CopyToClipboard()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in SelectedTagsList)
                sb.Append(item.Name).Append(' ');

            System.Windows.Clipboard.SetText(sb.ToString());
            StatusMessageService.ChangeStatusMessage("Copied to clipboard");
        }

        private void RemoveSelectedItems(System.Collections.IList itemsToRemove)
        {
            if (itemsToRemove.Count < 1)
                return;

            foreach (var item in itemsToRemove.Cast<TagRecord>().ToList())
                SelectedTagsList.Remove(item);
        }

        private void AddToSelected(System.Collections.IList itemsToAdd)
        {
            if (itemsToAdd.Count < 1)
                return;

            foreach (var item in itemsToAdd.Cast<TagRecord>().ToList())
            {
                var existing = SelectedTagsList.FirstOrDefault(i => i.Name == item.Name);

                if (existing == null)
                    SelectedTagsList.Add(item);
            }
        }

        private async void OnGetTagsCommand(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                StatusMessageService.ChangeStatusMessage("Entered username is incorrect");
                return;
            }

            ViewManager.MainViewModel.IsOverlayVisible = true;
            StatusMessageService.ChangeStatusMessage("Loading tags");

            SavePreferences();

            var list = await InstaApiService.GetTagsFromList(username, PagesToLoad, IncludeGlobalCount);

            if (list == null)
            {
                IsContentAvailable = false;
                StatusMessageService.ChangeStatusMessage("Getting tags failed");
                ViewManager.MainViewModel.IsOverlayVisible = false;
                return;
            }

            TagsList = list.OrderByDescending(i => i.Count).Take(TagLimit).ToList();

            if (TagsList.Count > 0)
            {
                IsContentAvailable = true;
                StatusMessageService.Done();
            }
            else
            {
                IsContentAvailable = false;
                StatusMessageService.ChangeStatusMessage("No posts");
            }

            ViewManager.MainViewModel.IsOverlayVisible = false;
        }

        #region Preferences

        private void ReadPreferences()
        {
            try
            {
                PagesToLoad = Convert.ToInt32(File.ReadAllText(PREF_DIRECTORY + PAGES_FILENAME));
                TagLimit = Convert.ToInt32(File.ReadAllText(PREF_DIRECTORY + TAGLIMIT_FILENAME));

            }
            catch (Exception)
            {
                Logger.Log("Reading preferences failed");
            }

        }

        private void SavePreferences()
        {
            try
            {
                Directory.CreateDirectory(PREF_DIRECTORY);

                File.WriteAllTextAsync(PREF_DIRECTORY + PAGES_FILENAME, PagesToLoad.ToString());
                File.WriteAllTextAsync(PREF_DIRECTORY + TAGLIMIT_FILENAME, TagLimit.ToString());
            }
            catch (Exception)
            {
                Logger.Log("Saving preferences failed");
            }
        }

        #endregion
    }
}
