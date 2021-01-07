using System.Collections.Generic;
using System.Threading.Tasks;

namespace TagFinder.Core.InstagramAPI
{
    public interface IInstagramAPI
    {
        public string CurrentUserName { get; }
        //public string UserProfilePicUrl { get; }

        public Task<LoginResult> LogInAsync(string userName, string password = null);
        public Task ProvidePhoneNumberAsync(string phoneNumber);
        public Task<LoginResult> ProvideVerificationCodeAsync(string code);
        public Task LogOutAsync();
        public Task<List<TagRecord>> GetTagsFromListAsync(string username, int pagesToLoad, bool includeGlobalCount = false);
        public Task<long> GetGlobalHashtagUsesAsync(string hashtag);
        public Task<string> DownloadUserProfilePicAsync(string userName);
    }
}
