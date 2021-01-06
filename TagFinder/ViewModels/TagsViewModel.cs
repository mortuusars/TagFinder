﻿using System;
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
using TagFinder.Logger;

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

        public string Username { get; set; }

        public string Status { get; set; } = StatusManager.Status;
        public int SelectedCount { get; set; }

        public bool IsUserInfoAvailable { get; set; }
        public string LoggedUsername { get; set; }
        public BitmapImage UserProfilePic { get; set; } = Utility.GetDefaultProfilePic();

        public bool ShowingCustomTagPanel { get; set; }
        public string CustomTag { get; set; }
        public bool IsCustomTagBoxFocused { get; set; }

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

        private IInstagramAPI _instagramAPI;
        private PageManager _pageManager;
        private ILogger _logger;



        public TagsViewModel(IInstagramAPI instagramAPI, PageManager pageManager, ILogger logger)
        {
            _instagramAPI = instagramAPI;
            _pageManager = pageManager;
            _logger = logger;

            #region Commands

            GetTagsCommand = new RelayCommand(_ => OnGetTagsCommand((string)Username), canEx => !string.IsNullOrWhiteSpace(Username));

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

            if (_instagramAPI.UserProfilePic != null)
                UserProfilePic = _instagramAPI.UserProfilePic;

            IsUserInfoAvailable = true;

            ReadPreferences();

#if DEBUG
            Username = "mortuus_cg";
#endif
        }

        

        private void OnSelectedChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SelectedCount = SelectedTagsList.Count > 0 ? SelectedTagsList.Count : 0;
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

                if (!Utility.CheckInternetAvailability())
                    StatusManager.Status = "Not connected to internet";
                else 
                    StatusManager.Status = "Getting tags failed";

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

            if (string.IsNullOrWhiteSpace(name))
            {
                //TODO error to user
                return;
            }

            name = name.StartsWith('#') ? name : '#' + name;

            var existing = SelectedTagsList.FirstOrDefault(i => i.Name == name);
            if (existing != null)
            {
                // TODO Error to user
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
