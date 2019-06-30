using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLibrary.Models
{
    public class TwitchCookie
    {
        public string name { get; set; }
        public string value { get; set; }
        public string host_key { get; set; }
    }
}
