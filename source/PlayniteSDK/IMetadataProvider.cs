using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    public interface IMetadataProvider
    {
        ICollection<MetadataSearchResult> SearchMetadata(Game game);
        GameMetadata GetMetadata(string metadataId);
        GameMetadata GetMetadata(Game game);
    }
}
