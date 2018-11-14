using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLibrary.Models
{
    public class TwitchOauthRequest
    {
        public string ClientID;
        public string Code;
        public string State;
        public string RedirectUri;
    }
}
