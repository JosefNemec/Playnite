using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.API
{
    public class PlayniteSettingsAPI : IPlayniteSettingsAPI
    {
        private readonly PlayniteSettings settings;

        public PlayniteSettingsAPI(PlayniteSettings settings)
        {
            this.settings = settings;
        }

        public bool GetGameExcludedFromImport(string gameId, Guid libraryId)
        {
            return settings.ImportExclusionList.Items.Any(a => a.GameId == gameId && a.LibraryId == libraryId);
        }
    }
}
