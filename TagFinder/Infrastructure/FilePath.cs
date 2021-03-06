﻿using System;

namespace TagFinder
{
    public static class FilePath
    {
        /// <summary>
        /// Path to application folder. Ends with /.
        /// </summary>
        public static readonly string APPLOCAL_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + $"/{Program.APP_NAME}/";
        /// <summary>
        /// Path to Cache folder. Ends with /.
        /// </summary>
        public static readonly string CACHE = APPLOCAL_FOLDER + "Cache/";

        public static readonly string SETTINGS_FILE = APPLOCAL_FOLDER + "settings.json";

        public static readonly string STATE_FILEPATH = CACHE + "state.bin";
        public static readonly string LAST_USER_FILEPATH = CACHE + "lastUser.";
        public static readonly string PAGES_TO_LOAD_FILEPATH = CACHE + "pagesToLoad.";
        public static readonly string TAG_LIMIT_FILEPATH = CACHE + "tagLimit.";

        public static readonly string LOG_FILE = APPLOCAL_FOLDER + "log.txt";

        public const string CHANGELOG_FILE = "changelog.md";
        public const string LATEST_CHANGELOG_FILE = "latestChangelog.md";
        public const string UPDATE_URL_FILE = "updateUrl.txt";
        public const string MANUAL_UPDATE_URL_FILE = "manualUpdateUrl.txt";
    }
}
