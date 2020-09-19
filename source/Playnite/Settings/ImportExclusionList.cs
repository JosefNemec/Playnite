using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class ImportExclusionItem
    {
        public string GameId { get; set; }
        public string GameName { get; set; }
        public Guid LibraryId { get; set; }
        public string LibraryName { get; set; }

        public ImportExclusionItem()
        {
        }

        public ImportExclusionItem(string gameId, string gameName, Guid libraryId, string libraryName)
        {
            GameId = gameId;
            GameName = gameName;
            LibraryId = libraryId;
            LibraryName = libraryName;
        }
    }

    public class ImportExclusionList
    {
        public ObservableCollection<ImportExclusionItem> Items { get; set; }

        public ImportExclusionList()
        {
            Items = new ObservableCollection<ImportExclusionItem>();
        }

        public void Add(string gameId, string gameName, Guid libraryId, string libraryName)
        {
            if (!Items.Any(a => a.GameId == gameId))
            {
                Items.Add(new ImportExclusionItem(gameId, gameName, libraryId, libraryName));
            }
        }
    }
}
