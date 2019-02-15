using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLibrary.Models
{
    public class TwitchOauthSession
    {
        public long UserID;
        public string Username;
        public string DisplayName;
        public string SessionID;
        public string Token;
        public string EmailAddress;
        public bool EffectivePremiumStatus;
        public bool ActualPremiumStatus;
        public long SubscriptionToken;
        public long Expires;
        public long RenewAfter;
        public bool IsTemporaryAccount;
        public bool IsMerged;
        public long Bans;
    }

    public class TwitchOauthResponse
    {
        public long Status;
        public TwitchOauthSession Session;
        public long Timestamp;
        public object MergeToken;
        public string TwitchUsername;
        public string TwitchDisplayName;
        public string TwitchAvatar;
        public string TwitchUserID;
    }
}
