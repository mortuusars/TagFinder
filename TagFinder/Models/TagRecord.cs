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

        public async void GetGlobalCountString()
        {
            long count = await InstaApiService.GetGlobalHashtagUses(Name.Remove(0, 1));

            if (count < 0)
                GlobalCount = "";

            long ths = count / 1000;

            if (ths >= 1000 )
                GlobalCount = ths / 1000 + "m";
            else if (ths >= 1)
                GlobalCount = ths + "k";
            else 
                GlobalCount = count.ToString();
        }
    }
}
