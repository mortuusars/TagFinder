using System;
using System.Collections.Generic;

namespace TagFinder.Core.InstagramAPI
{
    public class PostData
    {
        public int MediaCount { get; set; }
        public int Pages { get; set; }
        public int TagCount { get; set; }
        public DateTime LastPostDate { get; set; }
        public List<TagRecord> Tags { get; set; }
    }
}
