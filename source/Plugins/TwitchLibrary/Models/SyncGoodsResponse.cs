using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLibrary.Models
{
    public class Product
    {
        public string asin;
        public int asinVersion;
        public string id;
        public string productLine;
        public string productTitle;
        public string sku;
        public string type;
    }

    public class GoodsItem
    {
        public Product product;
    }

    public class SyncGoodsResponse
    {
        public List<GoodsItem> goods;
    }
}
