using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using TagFinder.Infrastructure;
using TagFinder.Views.Pages;
using Test;

namespace TagFinder.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsOverlayVisible { get; set; }
        public string Status { get; set; }

        public ICommand GetTagsCommand { get; }

        public MainViewModel()
        {
            GetTagsCommand = new RelayCommand(_ => GetTags());

            StatusMessageService.StatusChanged += (s, e) => Status = e;
        }

        private async void GetTags()
        {
            //IsOverlayVisible = true;
            //Status = "Getting posts info...";
            //var desc = await Program.GetPostDescriptionsFromUser(Username);

            //if (desc != null && desc.Count > 0)
            //{
            //    var resultList = Program.GetTagsFromList(desc).OrderByDescending(p => p.Count);

            //    Tags.Clear();

            //    int tagsCount = 0;
            //    foreach (var item in resultList)
            //    {
            //        if (tagsCount > TagsLimit)
            //            break;

            //        Tags.Add(new TagRecord() { Name = item.Name, Count = item.Count });
            //        tagsCount++;
            //    }
            //    Status = "";
            //}
            //else
            //{
            //    Status = "No posts or account is not exists";
            //}
            
            //IsOverlayVisible = false;
        }
    }
}
