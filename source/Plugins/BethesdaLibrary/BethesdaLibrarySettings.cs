﻿using Playnite;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BethesdaLibrary
{
    public class BethesdaLibrarySettings : ObservableObject, ISettings
    {
        private BethesdaLibrarySettings editingClone;
        private IPlayniteAPI api;
        private BethesdaLibrary library;

        #region Settings      

        public bool ImportInstalledGames { get; set; } = true;

        #endregion Settings

        public BethesdaLibrarySettings()
        {
        }

        public BethesdaLibrarySettings(BethesdaLibrary library, IPlayniteAPI api)
        {
            this.library = library;
            this.api = api;

            var settings = api.LoadPluginSettings<BethesdaLibrarySettings>(library);
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
            errors = null;
            return true;
        }

        private void LoadValues(BethesdaLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }
    }
}
