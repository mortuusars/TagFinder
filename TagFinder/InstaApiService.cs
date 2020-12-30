using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using TagFinder.Infrastructure;

namespace TagFinder
{
    public static class InstaApiService
    {
        public static event EventHandler LoggedIn;
        public static event EventHandler LoggedOut;

        private const string stateFile = "state.bin";

        public static string LoggedUsername { get; set; }

        private static IInstaApi _instaApi;

        #region Log In 

        public static async Task<LogInRequiredActions> LogIn(string userName, string password)
        {
            CreateInstaApiInstance(userName, password);

            if (!_instaApi.IsUserAuthenticated)
            {
                Logger.Log($"Logging in as {userName}");
                StatusMessageService.ChangeStatusMessage($"Logging in as {userName}");

                var logInResult = await _instaApi.LoginAsync();

                if (!logInResult.Succeeded)
                {
                    Logger.Log($"Unable to login: {logInResult.Info.Message}");

                    if (logInResult.Value == InstaLoginResult.ChallengeRequired)
                    {
                        Logger.Log("Getting login challenge...");
                        var challenge = await _instaApi.GetChallengeRequireVerifyMethodAsync();
                        if (challenge.Succeeded)
                        {
                            if (challenge.Value.SubmitPhoneRequired)
                            {
                                Logger.Log("Phone number required");
                                return LogInRequiredActions.PhoneNumberRequired;
                            }
                            else
                            {
                                if (challenge.Value.StepData != null)
                                {
                                    if (!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber))
                                    {
                                        // send verification code to phone number
                                        Logger.Log("Sending verification code by SMS...");
                                        var smsVerify = await _instaApi.RequestVerifyCodeToSMSForChallengeRequireAsync();

                                        if (smsVerify.Succeeded)
                                        {
                                            Logger.Log("Code sent by SMS");
                                            return LogInRequiredActions.SMSVerifyRequired;
                                        }
                                        else
                                        {
                                            Logger.Log(smsVerify.Info.Message);
                                            return LogInRequiredActions.Error;
                                        }

                                    }
                                    if (!string.IsNullOrEmpty(challenge.Value.StepData.Email))
                                    {
                                        Logger.Log("Sending verification code by Email...");
                                        var emailVerify = await _instaApi.RequestVerifyCodeToEmailForChallengeRequireAsync();

                                        if (emailVerify.Succeeded)
                                        {
                                            Logger.Log("Code sent to email");
                                            return LogInRequiredActions.EmailVerifyRequired;
                                        }
                                        else
                                        {
                                            Logger.Log(emailVerify.Info.Message);
                                            return LogInRequiredActions.Error;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Logger.Log(challenge.Info.Message);
                            return LogInRequiredActions.VerificationFailed;
                        }
                    }
                    else if (logInResult.Value == InstaLoginResult.BadPassword || logInResult.Value == InstaLoginResult.InvalidUser)
                    {
                        Logger.Log("Username of password is invalid");
                        return LogInRequiredActions.Error;
                    }
                    else
                    {
                        Logger.Log(logInResult.Info.Message);
                        return LogInRequiredActions.Error;
                    }
                }
            }

            await DownloadUserProfilePic(userName);

            SaveSession();

            Logger.Log("Logged in successfully");
            return LogInRequiredActions.Success;
        }

        public static LogInRequiredActions TryLogInWithLastUser(string userName)
        {
            Logger.Log("Trying to log with existing user data");
            CreateInstaApiInstance(userName);

            TryLoadSavedUserState();

            if (_instaApi.IsUserAuthenticated)
            {
                SaveSession();
                Logger.Log("Successfully restored previous user login data");
                return LogInRequiredActions.Success;
            }
            else
            {
                Logger.Log("Failed to login as last user. Full login required.");
                return LogInRequiredActions.FullLogInReqired;
            }
        }

        public static async Task ProvidePhoneNumber(string phoneNumber)
        {
            var submitPhone = await _instaApi.SubmitPhoneNumberForChallengeRequireAsync(phoneNumber);
            if (submitPhone.Succeeded)
            {
                Logger.Log("Phonenumber for Challenge submitted");
            }
            else
            {
                Logger.Log(submitPhone.Info.Message);
            }
        }

        public static async Task<LogInRequiredActions> ProvideVerificationCode(string code)
        {
            Logger.Log("Verifying entered code...");
            var verifyLogin = await _instaApi.VerifyCodeForChallengeRequireAsync(code);
            if (verifyLogin.Succeeded)
            {
                Logger.Log("Verification successful");
                SaveSession();
                return LogInRequiredActions.Success;
            }
            else if (verifyLogin.Value == InstaLoginResult.TwoFactorRequired)
            {
                Logger.Log("Two Factor required");
                return LogInRequiredActions.TwoFactorRequired;
            }
            else
            {
                Logger.Log(verifyLogin.Info.Message);
                return LogInRequiredActions.Error;
            }
        }

        private static void CreateInstaApiInstance(string userName = "", string password = "")
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

            _instaApi = InstaApiBuilder.CreateBuilder()
               .SetUser(userSession)
               .SetRequestDelay(RequestDelay.FromSeconds(1, 2))
               .Build();

            _instaApi.SetDevice(device);
        }

        private static void TryLoadSavedUserState()
        {
            try
            {
                if (File.Exists(stateFile))
                {
                    Logger.Log("Loading state from file");
                    using (var fileStream = File.OpenRead(stateFile))
                    {
                        _instaApi.LoadStateDataFromStream(fileStream);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
        }

        private static void SaveSession()
        {
            Logger.Log("Attempting to save session to file...");
            // Save session
            if (_instaApi == null)
                return;
            else if (!_instaApi.IsUserAuthenticated)
            {
                Logger.Log("Not authenticated");
                return;
            }
            else if (_instaApi.SessionHandler != null)
            {
                Logger.Log("Saving session handler...");
                _instaApi.SessionHandler.Save();
            }

            LoggedUsername = _instaApi.GetLoggedUser().UserName;
            SaveSessionFile();
        }

        private static void SaveSessionFile()
        {
            // save session in file
            var state = _instaApi.GetStateDataAsStream();
            // in .net core or uwp apps don't use GetStateDataAsStream.
            // use this one:
            // var state = _instaApi.GetStateDataAsString();
            // this returns you session as json string.
            using (var fileStream = File.Create(stateFile))
            {
                state.Seek(0, SeekOrigin.Begin);
                state.CopyTo(fileStream);
            }

            Logger.Log("Written session to file");
        }

        #endregion

        public static async Task LogOut()
        {
            Logger.Log("Logging Out...");
            var result = await _instaApi.LogoutAsync();
            Logger.Log($"Loged out: {result.Succeeded}");
        }

        private static async Task<List<string>> GetPostDataFromUser(string userName, int pagesToLoad)
        {
            Logger.Log($"Getting {pagesToLoad} pages of media from @{userName}...");
            var media = await _instaApi.UserProcessor.GetUserMediaAsync(userName, PaginationParameters.MaxPagesToLoad(pagesToLoad));

            if (media.Succeeded)
            {
                Logger.Log($"Found {media.Value.Count} posts");

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
                Logger.Log("Getting media failed");
                Logger.Log(media.Info.Message);
                return null;
            }
        }

        public static async Task<List<TagRecord>> GetTagsFromList(string username, int pagesToLoad, bool includeGlobalCount = false)
        {
            var list = await GetPostDataFromUser(username, pagesToLoad);

            List<TagRecord> tagList = new List<TagRecord>();

            if (list == null)
                return null;
            else if (list.Count < 1)
                return tagList;

            Logger.Log("Extracting tags from media...");

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

            Logger.Log($"Found {tagList.Count} tags");
            return tagList;
        }

        public static async Task<long> GetGlobalHashtagUses(string hashtag)
        {
            var hashtagInfo = await _instaApi.HashtagProcessor.GetHashtagInfoAsync(hashtag);
            return hashtagInfo.Succeeded ? hashtagInfo.Value.MediaCount : -1;
        }

        public static async Task<BitmapImage> DownloadUserProfilePic(string userName)
        {
            Logger.Log("Getting user profile picture");
            var userInfo = await _instaApi.UserProcessor.GetUserInfoByUsernameAsync(userName);

            if (userInfo.Succeeded)
            {
                Logger.Log("Downloading profile pic");
                return Utility.GetUserProfilePicFromUrl(userInfo.Value.ProfilePicUrl);

                //try
                //{
                //    Image orig;

                //    Logger.Log("Downloading profile pic");
                //    using (WebClient wc = new WebClient())
                //    {
                //        using (Stream s = wc.OpenRead(instaImage.Uri))
                //        {
                //            orig = Image.FromStream(s);
                //        }
                //    }

                //    using (Bitmap bmp = new Bitmap(48, 48))
                //    {
                //        using (var gr = Graphics.FromImage(bmp))
                //        {
                //            gr.DrawImage(orig, 0, 0, 48, 48);
                //            bmp.Save(FileNames.UserProfilePicFilePath, ImageFormat.Jpeg);
                //            Logger.Log("Saved profile pic to file");
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Logger.Log(ex.Message);
                //}
            }

            return Utility.GetDefaultProfilePic();
        }
    }
}
