using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLibrary.Models
{
    public class StoreListing
    {
        public string summary;
        public Image thumbnail;
        public List<Asset> assets;
        public Image hero_background;
        public Image header_background;
        public string description;
        public string id;
        public Sku sku;
        public string flavor_text;
    }

    public class Asset
    {
        public int width;
        public int height;
        public string id;
        public int size;
        public string mime_type;
    };

    public class Image
    {
        public int width;
        public int height;
        public long id;
        public string mime_type;
        public int size;
    }
}
