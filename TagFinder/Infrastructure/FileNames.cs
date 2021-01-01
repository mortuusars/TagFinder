﻿using System;

namespace TagFinder
{
    public static class FileNames
    {
        public static readonly string LOCAL_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string STATE_FILEPATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/TagFinder/state.bin";

        public const string CACHE_FOLDER = "Cache/";

        public const string LOG_FILE = "log.txt";

        public const string LAST_USER_FILEPATH = "Cache/lastUser.";
        public const string PAGES_TO_LOAD_FILEPATH = "Cache/pagesToLoad.";
        public const string TAG_LIMIT_FILEPATH = "Cache/tagLimit.";

        public const string USER_PROFILE_PIC_FILEPATH = "Cache/profilePic.jpg";
    }
}
