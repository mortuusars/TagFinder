using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFinder
{
    public static class Logger
    {
        public static void Log(string message)
        {
            File.AppendAllTextAsync(FileNames.LogFile, DateTimeOffset.Now.ToString("[HH:mm]yy-MM-dd---") + message + "\n");
        }
    }
}
