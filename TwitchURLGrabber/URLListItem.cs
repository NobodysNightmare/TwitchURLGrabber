using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitchURLGrabber
{
    class URLListItem
    {
        public DateTime FirstOccurence { get; private set; }

        public string URL { get; private set; }

        public int TotalCount { get; set; }

        public int UniqueCount { get; set; }

        public URLListItem(DateTime firstSeen, string url)
        {
            FirstOccurence = firstSeen;
            URL = url;
            TotalCount = 1;
            UniqueCount = 1;
        }
    }
}
