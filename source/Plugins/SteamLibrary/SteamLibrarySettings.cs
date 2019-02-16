using Playnite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayniteUI.Commands;
using SteamLibrary.Models;
using Playnite.SDK;

namespace SteamLibrary
{
    public enum BackgroundSource
    {
        Image,
        StoreScreenshot,
        StoreBackground
    }

    public enum SteamIdSource
    {
        Name,
        LocalUser
    }

    public class SteamLibrarySettings : ISettings
    {
        private SteamLibrarySettings editingClone;
        private SteamLibrary library;
        private IPlayniteAPI api;

        #region Settings

        public SteamIdSource IdSource { get; set; } = SteamIdSource.LocalUser;

        public ulong AccountId { get; set; }

        public string AccountName { get; set; } = string.Empty;

        public bool IsPrivateAccount { get; set; } = false;

        public string ApiKey { get; set; } = string.Empty;

        public bool ImportInstalledGames { get; set; } = true;

        public bool ImportUninstalledGames { get; set; } = false;

        public BackgroundSource BackgroundSource { get; set; } = BackgroundSource.Image;

        #endregion Settings

        [JsonIgnore]
        public bool ShowCategoryImport { get; set; }

        [JsonIgnore]
        public List<LocalSteamUser> SteamUsers { get; set; }

        [JsonIgnore]
        public RelayCommand<LocalSteamUser> ImportSteamCategoriesCommand
        {
            get => new RelayCommand<LocalSteamUser>((a) =>
            {
                ImportSteamCategories(a);
            });
        }

        public SteamLibrarySettings()
        {
        }

        public SteamLibrarySettings(SteamLibrary library, IPlayniteAPI api)
        {
            this.library = library;
            this.api = api;

            var settings = api.LoadPluginSettings<SteamLibrarySettings>(library);
            if (settings != null)
            {
                LoadValues(settings);
            }            
        }

        public void BeginEdit()
        {
            editingClone = this.CloneJson();
        }

        public void CancelEdit()
        {
            LoadValues(editingClone);
        }

        public void EndEdit()
        {
            api.SavePluginSettings(library, this);
        }

        public bool VerifySettings(out List<string> errors)
        {
            var allValid = true;
            errors = new List<string>();

            if (ImportUninstalledGames && IdSource == SteamIdSource.Name && string.IsNullOrEmpty(AccountName))
            {
                errors.Add(api.Resources.FindString("LOCSettingsInvalidAccountName"));
                allValid = false;
            }

            return allValid;
        }

        private void LoadValues(SteamLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        public void ImportSteamCategories(LocalSteamUser user)
        {
            var accId = user == null ? 0 : user.Id;
            library.ImportSteamCategories(accId);
        }
    }
}
