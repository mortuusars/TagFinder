using System.ComponentModel;

namespace TagFinder.Core.InstagramAPI
{
    public class TagRecord : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; init; }
        public int Count { get; set; }
        public bool IsSelected { get; set; }
        public string GlobalCount { get; set; }
    }
}
