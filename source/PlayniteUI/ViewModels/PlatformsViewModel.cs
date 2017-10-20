using Playnite;
using Playnite.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUI.ViewModels
{
    public class PlatformsViewModel : ObservableObject
    {
        private GameDatabase database;
        public PlatformsViewModel(GameDatabase database)
        {
            this.database = database;
        }

        public void OpenPlatformsConfiguration()
        {
            var window = new PlatformsWindow();
            window.Owner = PlayniteWindows.CurrentWindow;
            window.ConfigurePlatforms(GameDatabase.Instance);
        }
    }
}
