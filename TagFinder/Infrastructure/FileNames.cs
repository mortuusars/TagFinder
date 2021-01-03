using System;

namespace TagFinder
{
    public static class FileNames
    {
        public static readonly string LOCAL_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string STATE_FILEPATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/TagFinder/state.bin";

        public const string CACHE_FOLDER = "Cache/";

        public const string CURRENT_VERSION_FILE = "version.json";
        public const string CHANGELOG_FILE = "changelog.md";
        public const string UPDATE_URL_FILE = "updateUrl.txt";
        public const string DOWNLOAD_URL_FILE = "updateDownloadPage.txt";

        public const string LOG_FILE = "log.txt";

        public const string SETTINGS_FILE = "settings.json";

        public const string LAST_USER_FILEPATH = "Cache/lastUser.";
        public const string PAGES_TO_LOAD_FILEPATH = "Cache/pagesToLoad.";
        public const string TAG_LIMIT_FILEPATH = "Cache/tagLimit.";

        public const string USER_PROFILE_PIC_FILEPATH = "Cache/profilePic.jpg";
    }
}
