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
using TagFinder.InstagramAPI;

namespace TagFinder.ViewModels
{
    public class TagsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<TagRecord> TagsList { get; set; }
        public ObservableCollection<TagRecord> SelectedTagsList { get; set; } = new ObservableCollection<TagRecord>();

        public bool IsContentAvailable { get; set; }

        public int PagesToLoad { get; set; } = 5;
        public int TagLimit { get; set; } = 20;
        public bool IncludeGlobalCount { get; set; } = false;

        public string Status { get; set; } = StatusManager.Status;
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

        private IInstagramAPI _instagramAPI;
        private PageManager _pageManager;
        private ILogger _logger;

        public TagsViewModel(IInstagramAPI instagramAPI, PageManager pageManager, ILogger logger)
        {
            _instagramAPI = instagramAPI;
            _pageManager = pageManager;
            _logger = logger;

            GetTagsCommand = new RelayCommand(username => OnGetTagsCommand((string)username));
            ClearSelectedCommand = new RelayCommand(_ => SelectedTagsList.Clear());
            RemoveSelectedCommand = new RelayCommand(items => RemoveSelectedItems((System.Collections.IList)items));
            AddToSelectedCommand = new RelayCommand(items => AddToSelected((System.Collections.IList)items));
            CopySelectedCommand = new RelayCommand(_ => CopyToClipboard());
            LogOutCommand = new RelayCommand(_ => OnLogOutCommand());
            
            StatusManager.StatusChanged += (_, e) => Status = StatusManager.Status;

            SelectedTagsList.CollectionChanged += OnSelectedChanged;

            if (_instagramAPI.UserProfilePic != null)
                UserProfilePic = _instagramAPI.UserProfilePic;

            IsUserInfoAvailable = true;

            ReadPreferences();
        }

        private async void OnLogOutCommand()
        {
            StatusManager.Status = "Logging out";
            StatusManager.InProgress = true;

            await _instagramAPI.LogOutAsync();

            _pageManager.SetPage(Views.Pages.Pages.LoginPage);

            StatusManager.Status = "Logged out";
            StatusManager.InProgress = false;
        }

        //private async void SetUserProfileInfoAsync()
        //{
        //    UserProfilePic = await StandardInstagramAPI.DownloadUserProfilePic(StandardInstagramAPI.LoggedUsername);
        //    LoggedUsername = StandardInstagramAPI.LoggedUsername;
        //    IsUserInfoAvailable = true;
        //}

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

            StatusManager.Status = "Copied to clipboard";
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
                StatusManager.Status = "Entered username is incorrect";
                return;
            }

            StatusManager.Status = "Loading tags";
            StatusManager.InProgress = true;

            SavePreferences();

            var list = await _instagramAPI.GetTagsFromListAsync(username, PagesToLoad, IncludeGlobalCount);

            StatusManager.InProgress = false;

            if (list == null)
            {
                IsContentAvailable = false;
                StatusManager.Status  = "Getting tags failed";
                return;
            }

            TagsList = list.OrderByDescending(i => i.Count).Take(TagLimit).ToList();

            if (TagsList.Count > 0)
            {
                IsContentAvailable = true;
                StatusManager.Status = "Done";
            }
            else
            {
                IsContentAvailable = false;
                StatusManager.Status = "No posts";
            }
        }

        #region Preferences

        private void ReadPreferences()
        {
            try
            {
                PagesToLoad = Convert.ToInt32(File.ReadAllText(FileNames.PAGES_TO_LOAD_FILEPATH));
                TagLimit = Convert.ToInt32(File.ReadAllText(FileNames.TAG_LIMIT_FILEPATH));

            }
            catch (Exception)
            {
                _logger.Log("Reading preferences failed");
            }

        }

        private void SavePreferences()
        {
            try
            {
                File.WriteAllTextAsync(FileNames.PAGES_TO_LOAD_FILEPATH, PagesToLoad.ToString());
                File.WriteAllTextAsync(FileNames.TAG_LIMIT_FILEPATH, TagLimit.ToString());
            }
            catch (Exception)
            {
                _logger.Log("Saving preferences failed");
            }
        }

        #endregion
    }
}
