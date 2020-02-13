using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XboxLibrary.Models
{
    public class AuthenticationData
    {
        public string AccessToken;
        public string RefreshToken;
        public string ExpiresIn;
        public DateTime CreationDate;
        public string UserId;
        public string TokenType;
    }

    public class AuthorizationData
    {
        public class DisplayClaimsData
        {
            public class XuiData
            {
                public string uhs;
                public string usr;
                public string utr;
                public string prv;
                public string xid;
                public string gtg;
            }

            public List<XuiData> xui;
        }

        public string Token;
        public DateTime IssueInstant;
        public DateTime NotAfter;
        public DisplayClaimsData DisplayClaims;
    }

    public class AthenticationRequest
    {
        public class AthenticationRequestProperties
        {
            public string AuthMethod { get; set; } = "RPS";
            public string SiteName { get; set; } = "user.auth.xboxlive.com";
            public string RpsTicket { get; set; }
        }

        public string RelyingParty { get; set; } = @"http://auth.xboxlive.com";
        public string TokenType { get; set; } = "JWT";
        public AthenticationRequestProperties Properties { get; set; } = new AthenticationRequestProperties();
    }

    public class AuhtorizationRequest
    {
        public class AuhtorizationRequestProperties
        {
            public string SandboxId { get; set; } = "RETAIL";
            public List<string> UserTokens { get; set; }
        }

        public string RelyingParty { get; set; } = @"http://xboxlive.com";
        public string TokenType { get; set; } = "JWT";
        public AuhtorizationRequestProperties Properties { get; set; } = new AuhtorizationRequestProperties();
    }

    public class TitleHistoryResponse
    {
        public class Detail
        {
            public string description;
            public string publisherName;
            public string developerName;
            public DateTime? releaseDate;
            public int? minAge;
        }

        public class Title
        {
            public string titleId;
            public string pfn;
            public string type;
            public string name;
            public string windowsPhoneProductId;
            public string modernTitleId;
            public string mediaItemType;
            public Detail detail;
            public List<string> devices;
        }

        public string xuid;
        public List<Title> titles;
    }

    public class ProfileRequest
    {
        public List<string> settings;
        public List<ulong> userIds;
    }

    public class RefreshTokenResponse
    {
        public string token_type;
        public string scope;
        public string access_token;
        public string refresh_token;
        public string user_id;
    }
}
