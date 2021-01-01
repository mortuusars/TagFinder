using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TagFinder
{
    public static class Utility
    {
        public static BitmapImage GetDefaultProfilePic()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/user_48.png"));
        }

        public static BitmapImage GetUserProfilePicFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return GetDefaultProfilePic();

            return new BitmapImage(new Uri(url));
        }
    }
}
