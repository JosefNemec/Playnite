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
        private UplayLibrary library;

        #region Settings

        public bool ImportInstalledGames { get; set; } = true;

        public bool ImportUninstalledGames { get; set; } = Uplay.IsInstalled;

        #endregion Settings


        public UplayLibrarySettings()
        {
        }

        public UplayLibrarySettings(UplayLibrary library)
        {
            this.library = library;
            var settings = library.LoadPluginSettings<UplayLibrarySettings>();
            if (settings != null)
            {
                LoadValues(settings);
            }
        }

        public void BeginEdit()
        {
            editingClone = this.GetClone();
        }

        public void CancelEdit()
        {
            LoadValues(editingClone);
        }

        public void EndEdit()
        {
            library.SavePluginSettings(this);
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
