using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitchURLGrabber
{
    class MessageEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public string User { get; private set; }

        public MessageEventArgs(string user, string message)
        {
            Message = message;
            User = user;
        }
    }
}
