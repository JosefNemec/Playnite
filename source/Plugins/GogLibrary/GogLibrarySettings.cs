﻿using Playnite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayniteUI.Commands;
using Playnite.SDK;
using GogLibrary.Services;

namespace GogLibrary
{
    public class GogLibrarySettings : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private GogLibrarySettings editingClone;
        private readonly GogLibrary library;
        private readonly IPlayniteAPI api;

        #region Settings

        public bool ImportInstalledGames { get; set; } = true;

        public bool ImportUninstalledGames { get; set; } = false;

        public string AccountName { get; set; }

        public bool UsePublicAccount { get; set; } = false;

        public bool StartGamesUsingGalaxy { get; set; } = false;

        #endregion Settings

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                using (var view = api.WebViews.CreateOffscreenView())
                {
                    var api = new GogAccountClient(view);
                    return api.GetIsUserLoggedIn();
                }
            }
        }

        [JsonIgnore]
        public RelayCommand<object> LoginCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Login();
            });
        }

        public GogLibrarySettings()
        {
        }

        public GogLibrarySettings(GogLibrary library, IPlayniteAPI api)
        {
            this.api = api;
            this.library = library;

            var settings = api.LoadPluginSettings<GogLibrarySettings>(library);
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

            if (ImportUninstalledGames && UsePublicAccount && string.IsNullOrEmpty(AccountName))
            {
                errors.Add(api.Resources.FindString("LOCSettingsInvalidAccountName"));
                allValid = false;
            }

            return allValid;
        }

        private void LoadValues(GogLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        private void Login()
        {
            try
            {
                using (var view = api.WebViews.CreateView(400, 445))
                {
                    var api = new GogAccountClient(view);
                    api.Login();
                }

                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Environment.IsDebugBuild)
            {
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}
