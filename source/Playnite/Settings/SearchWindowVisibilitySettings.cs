using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class SearchWindowVisibilitySettings : ObservableObject
    {
        private bool gameIcon = true;
        private bool libraryIcon = true;
        private bool hiddenStatus = true;
        private bool platform = true;
        private bool playTime = true;
        private bool completionStatus = true;
        private bool releaseDate = false;

        public bool GameIcon { get => gameIcon; set => SetValue(ref gameIcon, value); }
        public bool LibraryIcon { get => libraryIcon; set => SetValue(ref libraryIcon, value); }
        public bool HiddenStatus { get => hiddenStatus; set => SetValue(ref hiddenStatus, value); }
        public bool Platform { get => platform; set => SetValue(ref platform, value); }
        public bool PlayTime { get => playTime; set => SetValue(ref playTime, value); }
        public bool CompletionStatus { get => completionStatus; set => SetValue(ref completionStatus, value); }
        public bool ReleaseDate { get => releaseDate; set => SetValue(ref releaseDate, value); }
    }
}
