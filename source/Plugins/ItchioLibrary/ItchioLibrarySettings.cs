using Playnite;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary
{
    public class ItchioLibrarySettings : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private ItchioLibrarySettings editingClone;
        private ItchioLibrary library;
        private IPlayniteAPI api;

        #region Settings      

        public bool ImportInstalledGames { get; set; } = true;

        public bool ImportUninstalledGames { get; set; } = false;

        #endregion Settings

        public ItchioLibrarySettings()
        {
        }

        public ItchioLibrarySettings(ItchioLibrary library, IPlayniteAPI api)
        {
            this.library = library;
            this.api = api;

            var settings = api.LoadPluginSettings<ItchioLibrarySettings>(library);
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

        private void LoadValues(ItchioLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }
    }
}
