using System.ComponentModel;
using System.Threading.Tasks;

namespace TagFinder
{
    public class TagRecord : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; init; }
        public string GlobalCount { get; set; }
        public int Count { get; set; }
    }
}
