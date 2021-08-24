using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class ImportExclusionItem : DatabaseObject
    {
        public string GameId { get; set; }
        public Guid LibraryId { get; set; }
        public string LibraryName { get; set; }

        public ImportExclusionItem()
        {
        }

        public ImportExclusionItem(string gameId, string gameName, Guid libraryId, string libraryName)
        {
            GameId = gameId;
            Name = gameName;
            LibraryId = libraryId;
            LibraryName = libraryName;
            Id = GetId();
        }

        public Guid GetId()
        {
            return GetId(GameId, LibraryId);
        }

        public static Guid GetId(string gameId, Guid libraryId)
        {
            return new Guid($"{gameId}_{libraryId}".MD5Bytes());
        }
    }
}
