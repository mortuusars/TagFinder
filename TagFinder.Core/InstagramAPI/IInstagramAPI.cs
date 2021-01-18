using System.Collections.Generic;
using System.Threading.Tasks;

namespace TagFinder.Core.InstagramAPI
{
    public interface IInstagramAPI
    {
        string CurrentUserName { get; }

        Task<string> DownloadUserProfilePicAsync(string userName);
        Task<long> GetGlobalHashtagUsesAsync(string hashtag);
        List<TagRecord> GetTagsFromText(List<string> list);
        Task<PostData> GetUserPostDataAsync(string userName, int pagesToLoad);
        Task<LoginResult> LogInAsync(string userName, string password = null);
        Task LogOutAsync();
        Task ProvidePhoneNumberAsync(string phoneNumber);
        Task<LoginResult> ProvideVerificationCodeAsync(string code);
    }
}
