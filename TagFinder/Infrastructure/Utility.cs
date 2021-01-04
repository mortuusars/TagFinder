using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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

        public static bool CheckInternetAvailability()
        {
            string address = "https://www.google.com/";

            bool pingStatus = false;

            using (Ping p = new Ping())
            {
                string data = "check";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 120;

                try
                {
                    PingReply reply = p.Send(address, timeout, buffer);
                    pingStatus = reply.Status == IPStatus.Success;
                }
                catch (Exception)
                {
                    pingStatus = false;
                }

                return pingStatus;
            }
        }
    }
}
