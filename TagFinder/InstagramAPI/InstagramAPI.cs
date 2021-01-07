using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using TagFinder.Logger;

namespace TagFinder.InstagramAPI
{
    public class InstagramAPI : IInstagramAPI
    {
        public string CurrentUserName { get; private set; }
        public BitmapImage UserProfilePic { get; private set; }

        private IInstaApi _instaApi;

        private readonly string _userStateFilePath;
        private readonly ILogger _logger;

        public InstagramAPI(string userStateFilePath, ILogger logger)
        {
            _userStateFilePath = userStateFilePath;
            _logger = logger;
        }

        #region Log In 

        public async Task<LoginResult> LogInAsync(string userName, string password = null)
        {
            _instaApi = CreateInstaApiInstance(userName, password);

            if (password == null)
                return TryLogInWithLastUser(userName);

            if (!_instaApi.IsUserAuthenticated)
            {
                _logger.Log($"Logging in as {userName}...");

                var logInResult = await _instaApi.LoginAsync();

                if (!logInResult.Succeeded)
                {
                    _logger.Log($"Unable to login: {logInResult.Info.Message}");

                    if (logInResult.Value == InstaLoginResult.ChallengeRequired)
                    {
                        _logger.Log("Getting login challenge...");
                        var challenge = await _instaApi.GetChallengeRequireVerifyMethodAsync();
                        if (challenge.Succeeded)
                        {
                            if (challenge.Value.SubmitPhoneRequired)
                            {
                                _logger.Log("Phone number required");
                                return LoginResult.PhoneNumberRequired;
                            }
                            else
                            {
                                if (challenge.Value.StepData != null)
                                {
                                    if (!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber))
                                    {
                                        // send verification code to phone number
                                        _logger.Log("Sending verification code by SMS...");
                                        var smsVerify = await _instaApi.RequestVerifyCodeToSMSForChallengeRequireAsync();

                                        if (smsVerify.Succeeded)
                                        {
                                            _logger.Log("Code sent by SMS");
                                            return LoginResult.SMSVerifyRequired;
                                        }
                                        else
                                        {
                                            _logger.Log(smsVerify.Info.Message);
                                            return LoginResult.Error;
                                        }

                                    }
                                    if (!string.IsNullOrEmpty(challenge.Value.StepData.Email))
                                    {
                                        _logger.Log("Sending verification code by Email...");
                                        var emailVerify = await _instaApi.RequestVerifyCodeToEmailForChallengeRequireAsync();

                                        if (emailVerify.Succeeded)
                                        {
                                            _logger.Log("Code sent to email");
                                            return LoginResult.EmailVerifyRequired;
                                        }
                                        else
                                        {
                                            _logger.Log(emailVerify.Info.Message);
                                            return LoginResult.Error;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.Log(challenge.Info.Message);
                            return LoginResult.VerificationFailed;
                        }
                    }
                    else if (logInResult.Value == InstaLoginResult.BadPassword || logInResult.Value == InstaLoginResult.InvalidUser)
                    {
                        _logger.Log("Username of password is invalid");
                        return LoginResult.Error;
                    }
                    else
                    {
                        _logger.Log(logInResult.Info.Message);
                        return LoginResult.Error;
                    }
                }
            }

            UserProfilePic = await DownloadUserProfilePicAsync(userName);

            SaveSession();

            _logger.Log("Logged in successfully");
            return LoginResult.Success;
        }

        private LoginResult TryLogInWithLastUser(string userName)
        {
            _logger.Log("Trying to log with existing user data...");

            if (!File.Exists(_userStateFilePath))
            {
                _logger.Log("Saved user state file does not exists. Full login required.");
                return LoginResult.FullLogInReqired;
            }

            CreateInstaApiInstance(userName);

            TryLoadSavedUserState();

            if (_instaApi.IsUserAuthenticated)
            {
                SaveSession();
                _logger.Log("Successfully restored previous user login data");
                return LoginResult.Success;
            }
            else
            {
                _logger.Log("Failed to login as last user. Full login required.");
                return LoginResult.FullLogInReqired;
            }
        }

        public async Task ProvidePhoneNumberAsync(string phoneNumber)
        {
            var submitPhone = await _instaApi.SubmitPhoneNumberForChallengeRequireAsync(phoneNumber);
            if (submitPhone.Succeeded)
            {
                _logger.Log("Phonenumber for Challenge submitted");
            }
            else
            {
                _logger.Log(submitPhone.Info.Message);
            }
        }

        public async Task<LoginResult> ProvideVerificationCodeAsync(string code)
        {
            _logger.Log("Verifying entered code...");
            var verifyLogin = await _instaApi.VerifyCodeForChallengeRequireAsync(code);
            if (verifyLogin.Succeeded)
            {
                _logger.Log("Verification successful");
                SaveSession();
                return LoginResult.Success;
            }
            else if (verifyLogin.Value == InstaLoginResult.TwoFactorRequired)
            {
                _logger.Log("Two Factor required");
                return LoginResult.TwoFactorRequired;
            }
            else
            {
                _logger.Log(verifyLogin.Info.Message);
                return LoginResult.Error;
            }
        }

        private static IInstaApi CreateInstaApiInstance(string userName = "", string password = "")
        {
            UserSessionData userSession = new UserSessionData() { UserName = userName, Password = password };

            var device = new AndroidDevice
            {
                // Device name
                AndroidBoardName = "REDMI",
                // Device brand
                DeviceBrand = "XIAOMI",
                // Hardware manufacturer
                HardwareManufacturer = "XIAOMI",
                // Device model
                DeviceModel = "VINCE",
                // Device model identifier
                DeviceModelIdentifier = "Redmi 5 Plus",
                // Firmware brand
                FirmwareBrand = "HWPRA-H",
                // Hardware model
                HardwareModel = "hi6250",
                // Device guid
                DeviceGuid = new Guid("5923aead-348e-4111-abd4-327e2ae9fa19"),
                // Phone guid
                PhoneGuid = new Guid("5cc48043-810b-4cde-b24b-50ef608dc675"),
                // Device id based on Device guid
                DeviceId = ApiRequestMessage.GenerateDeviceIdFromGuid(new Guid("5923aead-348e-4111-abd4-327e2ae9fa19")),
                // Resolution
                Resolution = "1080x2160",
                // Dpi
                Dpi = "480dpi",
                // Device brand/android board name/device model : android version/hardware model/95414346:user/release-keys
                FirmwareFingerprint = "XIAOMI/REDMI/VINCE:11.0/hi6250/95414346:user/release-keys",
                // don't change this
                AndroidBootloader = "4.23",
                // don't change this
                DeviceModelBoot = "qcom",
                // don't change this
                FirmwareTags = "release-keys",
                // don't change this
                FirmwareType = "user"
            };

            var instaApi = InstaApiBuilder.CreateBuilder()
               .SetUser(userSession)
               .SetRequestDelay(RequestDelay.FromSeconds(1, 2))
               .Build();

            instaApi.SetDevice(device);

            return instaApi;
        }

        private void TryLoadSavedUserState()
        {
            try
            {
                _logger.Log("Loading state from file...");
                using (var fileStream = File.OpenRead(_userStateFilePath))
                {
                    _instaApi.LoadStateDataFromStream(fileStream);
                    _logger.Log("Loaded user state data");
                }
            }
            catch (Exception e)
            {
                _logger.Log(e.Message);
            }
        }

        private void SaveSession()
        {
            _logger.Log("Attempting to save session to file...");
            // Save session
            if (_instaApi == null)
                return;
            else if (!_instaApi.IsUserAuthenticated)
            {
                _logger.Log("Not authenticated");
                return;
            }
            else if (_instaApi.SessionHandler != null)
            {
                _logger.Log("Saving session handler...");
                _instaApi.SessionHandler.Save();
            }

            CurrentUserName = _instaApi.GetLoggedUser().UserName;
            SaveSessionFile();
        }

        private void SaveSessionFile()
        {
            // save session in file
            var state = _instaApi.GetStateDataAsStream();
            // in .net core or uwp apps don't use GetStateDataAsStream.
            // use this one:
            // var state = _instaApi.GetStateDataAsString();
            // this returns you session as json string.
            using (var fileStream = File.Create(_userStateFilePath))
            {
                state.Seek(0, SeekOrigin.Begin);
                state.CopyTo(fileStream);
            }

            _logger.Log("Written session to file");
        }

        #endregion

        public async Task LogOutAsync()
        {
            _logger.Log("Logging Out...");
            var result = await _instaApi.LogoutAsync();
            _logger.Log($"Loged out: {result.Succeeded}");
        }

        private async Task<List<string>> GetPostDataFromUserAsync(string userName, int pagesToLoad)
        {
            _logger.Log($"Getting {pagesToLoad} pages of media from @{userName}...");
            var media = await _instaApi.UserProcessor.GetUserMediaAsync(userName, PaginationParameters.MaxPagesToLoad(pagesToLoad));

            if (media.Succeeded)
            {
                _logger.Log($"Found {media.Value.Count} posts");

                List<string> postTexts = new List<string>();

                foreach (var item in media.Value)
                {
                    if (item.Caption != null)
                        postTexts.Add(item.Caption.Text);

                    //if (item.PreviewComments != null)
                    //{
                    //    var comments = await _instaApi.CommentProcessor.GetMediaCommentsAsync(item.InstaIdentifier, PaginationParameters.MaxPagesToLoad(2));

                    //    if (comments.Succeeded)
                    //        foreach (var comment in comments.Value.Comments)
                    //        {
                    //            if (comment.User.UserName == userName)
                    //                postTexts.Add(comment.Text);
                    //        }
                    //}
                }

                return postTexts;
            }
            else
            {
                _logger.Log("Getting media failed");
                _logger.Log(media.Info.Message);
                return null;
            }
        }

        public async Task<List<TagRecord>> GetTagsFromListAsync(string username, int pagesToLoad, bool includeGlobalCount = false)
        {
            var list = await GetPostDataFromUserAsync(username, pagesToLoad);

            List<TagRecord> tagList = new List<TagRecord>();

            if (list == null)
                return null;
            else if (list.Count < 1)
                return tagList;

            _logger.Log("Extracting tags from media...");

            foreach (var item in list)
            {
                foreach (var match in Regex.Matches(item, "#\\S+\\s?"))
                {
                    string hashtag = match.ToString().Trim();

                    var existingTag = tagList.Find(i => i.Name == hashtag);

                    if (existingTag != null)
                        existingTag.Count++;
                    else
                    {
                        tagList.Add(new TagRecord() { Name = hashtag, Count = 1 });
                    }
                }
            }

            _logger.Log($"Found {tagList.Count} tags");
            return tagList;
        }

        public async Task<long> GetGlobalHashtagUsesAsync(string hashtag)
        {
            var hashtagInfo = await _instaApi.HashtagProcessor.GetHashtagInfoAsync(hashtag);
            return hashtagInfo.Succeeded ? hashtagInfo.Value.MediaCount : -1;
        }

        public async Task<BitmapImage> DownloadUserProfilePicAsync(string userName)
        {
            _logger.Log("Getting user profile picture");
            var userInfo = await _instaApi.UserProcessor.GetUserInfoByUsernameAsync(userName);

            if (userInfo.Succeeded)
            {
                _logger.Log("Downloading profile pic");
                var pic = Utility.GetUserProfilePicFromUrl(userInfo.Value.ProfilePicUrl);
                UserProfilePic = pic;
                return pic;
            }

            return Utility.GetDefaultProfilePic();
        }

        //public async Task<string> GetUserTags()
        //{
        //    var param = PaginationParameters.MaxPagesToLoad(5);
        //    param. = 2;

        //    var result = await _instaApi.UserProcessor.GetUserMediaAsync(CurrentUserName, param);

        //    return "";
        //}
    }
}
