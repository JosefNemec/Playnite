using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary.Models
{
    public class OauthResponse
    {
        public string access_token;
        public long expires_in;
        public DateTime expires_at;
        public string token_type;
        public string refresh_token;
        public long refresh_expires;
        public DateTime refresh_expires_at;
        public string account_id;
        public string client_id;
        public bool internal_client;
        public string client_service;
        public string app;
        public string in_app_id;
        public string device_id;
    }
}
