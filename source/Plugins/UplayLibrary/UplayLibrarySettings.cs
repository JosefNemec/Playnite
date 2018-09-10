using Playnite;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UplayLibrary
{
    public class UplayLibrarySettings : ObservableObject, ISettings
    {
        private UplayLibrarySettings editingClone;
        private IPlayniteAPI api;
        private UplayLibrary library;

        #region Settings      

        public bool ImportInstalledGames { get; set; } = true;

        #endregion Settings


        public UplayLibrarySettings()
        {
        }

        public UplayLibrarySettings(UplayLibrary library, IPlayniteAPI api)
        {
            this.library = library;
            this.api = api;

            var settings = api.LoadPluginSettings<UplayLibrarySettings>(library);
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

        private void LoadValues(UplayLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }
    }
}
