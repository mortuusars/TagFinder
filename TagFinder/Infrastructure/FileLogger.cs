using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFinder
{
    public class FileLogger : ILogger
    {
        private string _filePath;

        public FileLogger(string filePath)
        {
            _filePath = filePath;
        }

        public void Log(string message)
        {
            File.AppendAllTextAsync(_filePath, DateTimeOffset.Now.ToString("[HH:mm]yy-MM-dd---") + message + "\n");
        }
    }
}
