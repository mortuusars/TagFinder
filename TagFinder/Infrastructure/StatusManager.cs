using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFinder
{
    public static class StatusManager
    {
        public static event EventHandler StatusChanged;
        public static event EventHandler InProgressChanged;

        private static bool inProgress;
        public static bool InProgress
        {
            get { return inProgress; }
            set { inProgress = value; InProgressChanged?.Invoke(null, EventArgs.Empty); }
        }

        private static string status;
        public static string Status
        {
            get { return status; }
            set { status = value; StatusChanged?.Invoke(null, EventArgs.Empty); }
        }
    }
}
