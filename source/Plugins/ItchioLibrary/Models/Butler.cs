using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary.Models.Butler
{
    public class ButlerOutput
    {
        public long time;
        public string type;
    }

    public class Log : ButlerOutput
    {
        public const string MessageType = "log";
        public string level;
        public string message;
    }

    public class ListenNotification : ButlerOutput
    {
        public const string MessageType = "butlerd/listen-notification";

        public class Tcp
        {
            public string address;
        }

        public Tcp tcp;
        public string secret;
    }
}
