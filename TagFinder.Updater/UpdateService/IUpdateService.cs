using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TagFinder.Updater
{
    public interface IUpdateService
    {
        public event EventHandler<string> UpdateProcess;

        /// <summary>
        /// Log with additional information about update process.
        /// </summary>
        public List<string> UpdateLog { get; }

        /// <summary>
        /// Initializes update process.
        /// </summary>
        /// <returns><see langword="true"/> if updated successfully.</returns>
        public Task<bool> Update();
    }
}
