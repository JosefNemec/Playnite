using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleNetLibrary.Models
{
    public enum BNetAppType
    {
        Default,
        Classic
    }
    public class BNetApp
    {
        public string ProductId;
        public string InternalId;
        public string IconUrl;
        public string BackgroundUrl;
        public string CoverUrl;
        public string Name;
        public BNetAppType Type;
        public string ClassicExecutable;
        public List<Link> Links;
        public long ApiId;
    }
}
