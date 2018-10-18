using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Settings.OldSettings
{
    public class BattleNetSettings
    {
        public bool LibraryDownloadEnabled { get; set; }
        public bool IntegrationEnabled { get; set; }
    }
    public class SteamSettings
    {
        public enum SteamIdSource
        {
            Name,
            LocalUser
        }

        public SteamIdSource IdSource { get; set; }
        public ulong AccountId { get; set; }
        public string AccountName { get; set; }
        public bool PrivateAccount { get; set; }
        public string APIKey { get; set; }
        public bool LibraryDownloadEnabled { get; set; }
        public bool IntegrationEnabled { get; set; }
        public bool PreferScreenshotForBackground { get; set; }
    }

    public class UplaySettings
    {
        public bool LibraryDownloadEnabled { get; set; }
        public bool IntegrationEnabled { get; set; }
    }

    public class OriginSettings
    {
        public bool LibraryDownloadEnabled { get; set; }
        public bool IntegrationEnabled { get; set; }
    }

    public class GogSettings
    {
        public bool LibraryDownloadEnabled { get; set; }
        public bool IntegrationEnabled { get; set; }
        public bool RunViaGalaxy { get; set; }
    }

    public class Settings
    {
        public SteamSettings SteamSettings { get; set; }
        public OriginSettings OriginSettings { get; set; }
        public UplaySettings UplaySettings { get; set; }
        public BattleNetSettings BattleNetSettings { get; set; }
        public GogSettings GOGSettings { get; set; }
    }
}
