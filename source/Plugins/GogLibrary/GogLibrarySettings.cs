using Playnite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GogLibrary
{
    public class GogLibrarySettings : IEditableObject
    {
        private GogLibrarySettings editingClone;
        private readonly string configPath;

        #region Settings

        public bool ImportUninstalledGames { get; set; } = false;

        public bool StartGamesUsingGalaxy { get; set; } = false;

        #endregion Settings

        public GogLibrarySettings()
        {
        }

        public GogLibrarySettings(string configPath)
        {
            this.configPath = configPath;
            if (File.Exists(configPath))
            {
                var strConf = File.ReadAllText(configPath);
                LoadValues(JsonConvert.DeserializeObject<GogLibrarySettings>(strConf));
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

        private void LoadValues(GogLibrarySettings source)
        {
            ImportUninstalledGames = source.ImportUninstalledGames;
            StartGamesUsingGalaxy = source.StartGamesUsingGalaxy;
        }
    }
}
