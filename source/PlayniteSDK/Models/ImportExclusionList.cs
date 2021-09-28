using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    /// <summary>
    /// Represents import exclusion item.
    /// </summary>
    public class ImportExclusionItem : DatabaseObject
    {
        /// <summary>
        /// Gets or sets game's store ID.
        /// </summary>
        public string GameId { get; set; }
        /// <summary>
        /// Gets or sets source plugin ID.
        /// </summary>
        public Guid LibraryId { get; set; }
        /// <summary>
        /// Gets or sets source plugin name.
        /// </summary>
        public string LibraryName { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="ImportExclusionItem"/>.
        /// </summary>
        public ImportExclusionItem()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="ImportExclusionItem"/>.
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="gameName"></param>
        /// <param name="libraryId"></param>
        /// <param name="libraryName"></param>
        public ImportExclusionItem(string gameId, string gameName, Guid libraryId, string libraryName)
        {
            GameId = gameId;
            Name = gameName;
            LibraryId = libraryId;
            LibraryName = libraryName;
            Id = GetId();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public Guid GetId()
        {
            return GetId(GameId, LibraryId);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="libraryId"></param>
        /// <returns></returns>
        public static Guid GetId(string gameId, Guid libraryId)
        {
            var id = $"{gameId}_{libraryId}";
            using (var provider = System.Security.Cryptography.MD5.Create())
            return new Guid(provider.ComputeHash(Encoding.UTF8.GetBytes(id)));
        }

        /// <inheritdoc/>
        public override void CopyDiffTo(object target)
        {
            base.CopyDiffTo(target);

            if (target is ImportExclusionItem tro)
            {
                if (!string.Equals(GameId, tro.GameId, StringComparison.Ordinal))
                {
                    tro.GameId = GameId;
                }

                if (!string.Equals(LibraryName, tro.LibraryName, StringComparison.Ordinal))
                {
                    tro.LibraryName = LibraryName;
                }

                if (LibraryId != tro.LibraryId)
                {
                    tro.LibraryId = LibraryId;
                }
            }
            else
            {
                throw new ArgumentException($"Target object has to be of type {GetType().Name}");
            }
        }
    }
}
