using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SteamLibrary
{
    public enum SteamIdSource
    {
        Name,
        LocalUser
    }

    public class SteamLibrarySettings : IEditableObject
    {
        public SteamIdSource IdSource
        {
            get; set;
        } = SteamIdSource.Name;

        public ulong AccountId
        {
            get; set;
        }

        public string AccountName
        {
            get; set;
        } = string.Empty;

        public bool IsPrivateAccount
        {
            get; set;
        } = false;

        public string APIKey
        {
            get; set;
        } = string.Empty;

        public bool DownloadUninstalledGames
        {
            get; set;
        } = false;

        public bool PreferScreenshotForBackground
        {
            get; set;
        } = false;

        public void BeginEdit()
        {
        }

        public void CancelEdit()
        {
        }

        public void EndEdit()
        {
        }
    }
}
