using System;
using System.Threading.Tasks;

namespace TagFinder.Updater
{
    public interface IUpdateService
    {
        public event EventHandler<string> UpdateProcess;
        public Task<bool> Update();
    }
}
