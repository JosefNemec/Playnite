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
using Playnite.SteamLibrary.Models;

namespace Playnite.SteamLibrary
{
    public enum SteamIdSource
    {
        Name,
        LocalUser
    }

    public class SteamLibrarySettings : IEditableObject
    {
        private SteamLibrarySettings editingClone;
        private readonly string configPath;

        #region Settings

        public SteamIdSource IdSource { get; set; } = SteamIdSource.Name;

        public ulong AccountId { get; set; }

        public string AccountName { get; set; } = string.Empty;

        public bool IsPrivateAccount { get; set; } = false;

        public string ApiKey { get; set; } = string.Empty;

        public bool ImportUninstalledGames { get; set; } = false;

        public bool PreferScreenshotForBackground { get; set; } = false;

        #endregion Settings

        [JsonIgnore]
        public List<LocalSteamUser> SteamUsers { get; set; }

        [JsonIgnore]
        public RelayCommand<object> ImportSteamCategoriesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ImportSteamCategories();
            });
        }

        public SteamLibrarySettings()
        {
        }

        public SteamLibrarySettings(string configPath)
        {
            this.configPath = configPath;
            if (File.Exists(configPath))
            {
                var strConf = File.ReadAllText(configPath);
                LoadValues(JsonConvert.DeserializeObject<SteamLibrarySettings>(strConf));
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
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(configPath, str);
        }

        private void LoadValues(SteamLibrarySettings source)
        {
            IdSource = source.IdSource;
            AccountId = source.AccountId;
            IsPrivateAccount = source.IsPrivateAccount;
            ApiKey = source.ApiKey;
            ImportUninstalledGames = source.ImportUninstalledGames;
            PreferScreenshotForBackground = source.PreferScreenshotForBackground;
        }

        public void ImportSteamCategories()
        {
            //if (dialogs.ShowMessage(
            //    resources.FindString("LOCSettingsSteamCatImportWarn"),
            //    resources.FindString("LOCSettingsSteamCatImportWarnTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            //{
            //    return;
            //}

            //if (Settings.SteamSettings.AccountId == 0)
            //{
            //    dialogs.ShowMessage(
            //        resources.FindString("LOCSettingsSteamCatImportErrorAccount"),
            //        resources.FindString("LOCImportError"),
            //        MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            //if (database.GamesCollection == null)
            //{
            //    dialogs.ShowMessage(
            //        resources.FindString("LOCSettingsSteamCatImportErrorDb"),
            //        resources.FindString("LOCImportError"),
            //        MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            //try
            //{
            //    var steamLib = new SteamLibrary();
            //    var games = steamLib.GetCategorizedGames(Settings.SteamSettings.AccountId);

            //    database.ImportCategories(games);
            //    dialogs.ShowMessage(resources.FindString("LOCImportCompleted"));
            //}
            //catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            //{
            //    logger.Error(exc, "Failed to import Steam categories.");
            //    dialogs.ShowMessage(
            //        resources.FindString("LOCSettingsSteamCatImportError"),
            //        resources.FindString("LOCImportError"),
            //        MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }
    }
}
