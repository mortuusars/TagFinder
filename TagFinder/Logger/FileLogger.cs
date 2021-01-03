using System;
using System.IO;
using System.Linq;

namespace TagFinder.Logger
{
    public class FileLogger : ILogger
    {
        private string _filePath;

        public FileLogger(string filePath)
        {
            _filePath = filePath;

            CheckFileSize();
        }

        private void CheckFileSize()
        {
            var info = new FileInfo(_filePath);

            if (info.Length > 10000)
            {
                var lines = File.ReadLines(_filePath).ToArray();
                var lastLines = lines.Skip(Math.Max(0, lines.Length - 50));

                File.WriteAllLines(_filePath, lastLines);
            }
        }

        public void Log(string message)
        {
            File.AppendAllTextAsync(_filePath, DateTimeOffset.Now.ToString("[HH:mm]yy-MM-dd---") + message + "\n");
        }
    }
}
