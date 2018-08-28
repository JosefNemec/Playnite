using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLibrary.Models
{
    public class FuelLoginResponse
    {
        public string AccessToken;
        public string RefreshToken;
        public long Expires;
        public object message;
        public object status;
        public object error;
    }
}
