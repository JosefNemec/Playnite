using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GogLibrary.Models
{
    public class AccountBasicRespose
    {
        public string accessToken;
        public int accessTokenExpires;
        public string avatar;
        public int cacheExpires;
        public string clientId;
        public bool isLoggedIn;
        public string userId;
        public string username;
    }
}
