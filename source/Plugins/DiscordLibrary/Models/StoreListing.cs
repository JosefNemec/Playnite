using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public long id;
        public Sku sku;
        public string flavor_text;

        public string htmlDescription()
        {
            // Convert asset:// markdown images to html images with corrected links
            Regex regex = new Regex(@"!\[\]\(asset://(\d+)\)");
            var descriptionMarkdown = regex.Replace(description, m =>
            {
                var assetId = long.Parse(m.Groups[1].Value);
                var asset = assets.First(a => a.id == assetId);
                // Need to parse extension, some images are gifs
                var imageExtension = asset.mime_type.Split('/')[1];
                // Images should be displayed block, centered, with space above and below
                return $"<center style=\"margin: 1em 0\"><img src=\"https://cdn.discordapp.com/app-assets/{sku.application_id}/store/{assetId}.{imageExtension}?size=1024\"></center>";
            });

            var markdown = new MarkdownDeep.Markdown()
            {
                ExtraMode = true,
                SafeMode = false,
                MarkdownInHtml = true
            };
            return markdown.Transform(descriptionMarkdown);
        }

    }

    public class Asset
    {
        public int width;
        public int height;
        public long id;
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
