using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace TwitchURLGrabber
{
    class URLListItem : INotifyPropertyChanged
    {
        public DateTime FirstOccurence { get; private set; }

        public string URL { get; private set; }

        private int _TotalCount;
        public int TotalCount
        {
            get { return _TotalCount; }
            set
            {
                _TotalCount = value;
                OnPropertyChanged("TotalCount");
            }
        }

        public HashSet<string> SentBy { get; private set; }

        public URLListItem(DateTime firstSeen, string url)
        {
            FirstOccurence = firstSeen;
            URL = url;
            SentBy = new HashSet<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
