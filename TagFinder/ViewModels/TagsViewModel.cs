using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PropertyChanged;
using TagFinder.Core.InstagramAPI;
using TagFinder.Core.Logger;
using TagFinder.Toasts;

namespace TagFinder.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    [SuppressPropertyChangedWarnings]
    public class TagsViewModel
    {
        #region Properties

        public List<TagRecord> TagsList { get; set; }
        public ObservableCollection<TagRecord> SelectedTagsList { get; set; } = new ObservableCollection<TagRecord>();

        public bool IsContentAvailable { get; set; }

        public int PagesToLoad { get; set; } = 5;
        public int TagLimit { get; set; } = 20;
        public bool IncludeGlobalCount { get; set; }

        public string GetTagsUsername { get; set; }

        public string Status { get; set; } = StatusManager.Status;
        public int SelectedCount { get; set; }

        public bool IsUserInfoAvailable { get; set; }
        public string LoggedUsername { get; set; }
        public BitmapImage UserProfilePic { get; private set; }

        public bool ShowingCustomTagPanel { get; set; }
        public string CustomTag { get; set; }
        public bool IsCustomTagBoxFocused { get; set; }

        #endregion

        #region Commands

        public ICommand GetTagsCommand { get; }
        public ICommand ClearSelectedCommand { get; }
        public ICommand RemoveItemFromSelectedCommand { get; }
        public ICommand RemoveListFromSelectedCommand { get; }
        public ICommand AddListToSelectedCommand { get; }
        public ICommand AddItemToSelectedCommand { get; }
        public ICommand AddCustomTagCommand { get; }
        public ICommand CopySelectedCommand { get; }

        public ICommand ShowAddTagPanelCommand { get; }
        public ICommand CloseAddingCustomTagCommand { get; }

        public ICommand SettingsCommand { get; }
        public ICommand LogOutCommand { get; }

        #endregion

        private IInstagramAPI _instagramAPI;
        private PageManager _pageManager;
        private IToastManager _toastManager;
        private ILogger _logger;

        public TagsViewModel(IInstagramAPI instagramAPI, PageManager pageManager, IToastManager toastManager, ILogger logger)
        {
            _instagramAPI = instagramAPI;
            _pageManager = pageManager;
            _toastManager = toastManager;
            _logger = logger;

            #region Commands

            GetTagsCommand = new RelayCommand(_ => OnGetTagsCommand((string)GetTagsUsername), canEx => !string.IsNullOrWhiteSpace(GetTagsUsername));

            RemoveItemFromSelectedCommand = new RelayCommand(item => RemoveItemFromSelected((TagRecord)item));
            RemoveListFromSelectedCommand = new RelayCommand(items => RemoveListFromSelectedItems((System.Collections.IList)items));
            AddItemToSelectedCommand = new RelayCommand(item => AddItemToSelected((TagRecord)item));
            AddListToSelectedCommand = new RelayCommand(items => AddListToSelected((System.Collections.IList)items));

            ClearSelectedCommand = new RelayCommand(_ => RemoveListFromSelectedItems(SelectedTagsList), canEx => SelectedCount > 0);
            CopySelectedCommand = new RelayCommand(_ => CopyToClipboard(), canEx => SelectedCount > 0);

            ShowAddTagPanelCommand = new RelayCommand(_ => ShowAddTagPanel());

            AddCustomTagCommand = new RelayCommand(_ => { AddCustomTag(CustomTag); CustomTag = string.Empty; }, canEx => CanAddCustomTag(CustomTag));
            CloseAddingCustomTagCommand = new RelayCommand(_ => { ShowingCustomTagPanel = false; CustomTag = string.Empty; IsCustomTagBoxFocused = false; });

            SettingsCommand = new RelayCommand(_ => ViewManager.ShowSettingsView());
            LogOutCommand = new RelayCommand(_ => OnLogOutCommand());

            #endregion

            StatusManager.StatusChanged += (_, e) => Status = StatusManager.Status;

            SelectedTagsList.CollectionChanged += OnSelectedChanged;

            GetUserProfilePic();

            ReadPreferences();

#if DEBUG
            GetTagsUsername = "mortuus_cg";
#endif
        }

        private void OnSelectedChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SelectedCount = SelectedTagsList.Count;
            if (SelectedCount > 30)
                _toastManager.ShowMessage("Tag limit exceeded", Resources.Icons.Error);
        }

        private async void GetUserProfilePic()
        {
            string picUrl = await _instagramAPI.DownloadUserProfilePicAsync(_instagramAPI.CurrentUserName);
            UserProfilePic = Utility.GetUserProfilePicFromUrl(picUrl);
            IsUserInfoAvailable = true;
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

        private async void OnGetTagsCommand(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _toastManager.ShowMessage("Entered username is incorrect", Resources.Icons.Error);
                return;
            }

            StatusManager.Status = "Loading tags";
            StatusManager.InProgress = true;

            SavePreferences();

            var userPostData = await _instagramAPI.GetUserPostDataAsync(username, PagesToLoad);

            _toastManager.ShowMessage($"Found {userPostData.MediaCount} posts up to {userPostData.LastPostDate:dd MMMM yyyy}");

            StatusManager.Clear();

            if (userPostData.Tags == null)
            {
                IsContentAvailable = false;

                if (!Utility.CheckInternetAvailability())
                    _toastManager.ShowMessage("Not connected to internet", Resources.Icons.Error);
                else
                    _toastManager.ShowMessage("Getting tags failed", Resources.Icons.Error);

                return;
            }

            if (userPostData.Tags.Count > 0)
            {
                IsContentAvailable = true;
            }
            else
            {
                IsContentAvailable = false;
                _toastManager.ShowMessage("No posts");
                return;
            }

            TagsList = userPostData.Tags.OrderByDescending(i => i.Count).Take(TagLimit).ToList();
        }

        #region ADD

        private void AddItemToSelected(TagRecord tagRecord)
        {
            var existing = SelectedTagsList.FirstOrDefault(i => i.Name == tagRecord.Name);

            if (existing == null)
            {
                tagRecord.IsSelected = true;
                SelectedTagsList.Add(tagRecord);
            }
        }

        private void AddListToSelected(System.Collections.IList itemsToAdd)
        {
            if (itemsToAdd.Count < 1)
                return;

            foreach (var item in itemsToAdd.Cast<TagRecord>().ToList())
            {
                AddItemToSelected(item);
            }
        }

        private void AddCustomTag(string name)
        {
            name = name.StartsWith('#') ? name : '#' + name;

            var existing = SelectedTagsList.FirstOrDefault(i => i.Name == name);
            if (existing != null)
            {
                // TODO Error Tag already added
                return;
            }

            // Check if manually added tag exists in main list.
            var existingInAll = TagsList.Find(i => i.Name == name);
            if (existingInAll != null)
            {
                SelectedTagsList.Add(existingInAll);
                existingInAll.IsSelected = true;
                return;
            }

            SelectedTagsList.Add(new TagRecord() { Name = name });
        }

        private void ShowAddTagPanel()
        {
            IsCustomTagBoxFocused = false;
            ShowingCustomTagPanel = !ShowingCustomTagPanel;
            IsCustomTagBoxFocused = true;
        }

        private bool CanAddCustomTag(string customTag) => !string.IsNullOrWhiteSpace(customTag);

        #endregion        

        #region REMOVE

        private void RemoveItemFromSelected(TagRecord tagRecord)
        {
            tagRecord.IsSelected = false;
            SelectedTagsList.Remove(tagRecord);
        }

        private void RemoveListFromSelectedItems(System.Collections.IList itemsToRemove)
        {
            if (itemsToRemove.Count < 1)
                return;

            foreach (var item in itemsToRemove.Cast<TagRecord>().ToList())
                RemoveItemFromSelected(item);
        }

        #endregion

        private void CopyToClipboard()
        {
            if (SelectedTagsList.Count == 0)
            {
                StatusManager.Status = "Nothing to copy";
                return;
            }

            StringBuilder sb = new StringBuilder();

            foreach (var item in SelectedTagsList)
                sb.Append(item.Name).Append(' ');

            System.Windows.Clipboard.SetText(sb.ToString());

            StatusManager.Status = "Copied to clipboard";
        }

        #region Preferences

        private void ReadPreferences()
        {
            try
            {
                PagesToLoad = Convert.ToInt32(File.ReadAllText(FilePath.PAGES_TO_LOAD_FILEPATH));
                TagLimit = Convert.ToInt32(File.ReadAllText(FilePath.TAG_LIMIT_FILEPATH));

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
                File.WriteAllTextAsync(FilePath.PAGES_TO_LOAD_FILEPATH, PagesToLoad.ToString());
                File.WriteAllTextAsync(FilePath.TAG_LIMIT_FILEPATH, TagLimit.ToString());
            }
            catch (Exception)
            {
                _logger.Log("Saving preferences failed");
            }
        }

        #endregion
    }
}
