using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary
{
    public class EpicMetadataProvider : ILibraryMetadataProvider
    {
        public EpicMetadataProvider()
        {
        }

        #region IMetadataProvider

        public GameMetadata GetMetadata(Game game)
        {
            return null;
        }

        #endregion IMetadataProvider
    }
}
