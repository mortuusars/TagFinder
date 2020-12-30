using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFinder
{
    public static class StatusMessageService
    {
        public static event EventHandler<string> StatusChanged;

        public static string Status { get; private set; }

        public static void ChangeStatusMessage(string message)
        {
            Status = message;
            StatusChanged?.Invoke(null, Status);
        }

        public static void Done() => ChangeStatusMessage("Done");
        public static void Error() => ChangeStatusMessage("Error");
    }
}
