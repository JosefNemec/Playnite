using Playnite.SDK.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginLibrary.Models
{
    public class OriginGameMetadata : GameMetadata
    {
        public GameStoreDataResponse StoreDetails
        {
            get; set;
        }
    }
}
