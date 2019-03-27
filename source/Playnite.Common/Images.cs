using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;

namespace Playnite.Common
{
    public class Images
    {
        public static Image GetImageFromResource(string path, string assemblyName, BitmapScalingMode scaling = BitmapScalingMode.HighQuality)
        {
            var image = new Image()
            {
                Source = new BitmapImage(new Uri($"pack://application:,,,/{assemblyName};component/" + path, UriKind.Absolute)),
                Height = 16,
                Width = 16
            };

            RenderOptions.SetBitmapScalingMode(image, scaling);
            return image;
        }

        public static Image GetImageFromFile(string path, BitmapScalingMode scaling = BitmapScalingMode.HighQuality)
        {
            var image = new Image()
            {
                Source = System.Drawing.Imaging.BitmapExtensions.BitmapFromFile(path),
                Height = 16,
                Width = 16
            };

            RenderOptions.SetBitmapScalingMode(image, scaling);
            return image;
        }

        public static Image GetEmptyImage(double height = 16, double width = 16)
        {
            return new Image()
            {
                Height = height,
                Width = width
            };
        }
    }
}
