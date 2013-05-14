using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitchURLGrabber
{
    class DisconnectedEventArgs : EventArgs
    {
        public bool IsFinal { get; private set; }

        public DisconnectedEventArgs(bool isFinal)
        {
            IsFinal = isFinal;
        }
    }
}
