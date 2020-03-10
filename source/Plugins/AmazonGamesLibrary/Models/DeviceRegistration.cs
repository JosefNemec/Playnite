using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonGamesLibrary.Models
{
    public class DeviceRegistrationRequest
    {
        public class RegistrationData
        {
            public string app_name;
            public string app_version;
            public string device_model;
            public string device_name;
            public string device_serial;
            public string device_type;
            public string domain;
            public string os_version;
        }

        public class AuthData
        {
            public string access_token;
        }

        public class UserContextMap
        {
        }

        public RegistrationData registration_data = new RegistrationData();
        public AuthData auth_data = new AuthData();
        public UserContextMap user_context_map = new UserContextMap();
        public List<string> requested_extensions;
        public List<string> requested_token_type;
    }

    public class DeviceRegistrationResponse
    {
        public class Response
        {
            public class Success
            {
                public class Tokens
                {
                    public class Bearer
                    {
                        public string access_token;
                        public string refresh_token;
                    }

                    public Bearer bearer;
                }

                public Tokens tokens;
            }

            public Success success;
        }

        public Response response;
        public string request_id;
    }

    public class ProfileInfo
    {
        public string user_id;
    }

    public class TokenRefreshRequest
    {
        public string source_token_type;
        public string requested_token_type;
        public string source_token;
        public string app_name;
        public string app_version;
    }
}
