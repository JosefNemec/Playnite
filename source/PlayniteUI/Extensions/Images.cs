using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;

namespace PlayniteUI
{
    public class Images
    {
        public static Image GetImageFromResource(string path)
        {
            var image = new Image()
            {
                Source = new BitmapImage(new Uri(@"pack://application:,,,/PlayniteUI;component/" + path, UriKind.Absolute)),
                Height = 16,
                Width = 16
            };

            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            return image;
        }

        public static Image GetEmptyImage()
        {
            return new Image()
            {
                Height = 16,
                Width = 16
            };
        }
    }
}
