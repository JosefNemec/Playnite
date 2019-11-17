using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;

namespace Playnite.Common
{
    public class ImageProperties
    {
        public int Height { get; set; }
        public int Width { get; set; }
    }

    public class Images
    {
        public static Image GetImageFromResource(string path, string assemblyName, BitmapScalingMode scaling = BitmapScalingMode.HighQuality, double height = 16, double width = 16)
        {
            var image = new Image()
            {
                Source = new BitmapImage(new Uri($"pack://application:,,,/{assemblyName};component/" + path, UriKind.Absolute)),
                Height = height,
                Width = width
            };

            RenderOptions.SetBitmapScalingMode(image, scaling);
            return image;
        }

        public static Image GetImageFromFile(string path, BitmapScalingMode scaling = BitmapScalingMode.HighQuality, double height = 16, double width = 16)
        {
            var image = new Image()
            {
                Source = System.Drawing.Imaging.BitmapExtensions.BitmapFromFile(path),
                Height = height,
                Width = width
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

        public static ImageProperties GetImageProperties(string imagePath)
        {
            var decoder = BitmapDecoder.Create(new Uri(imagePath) , BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
            if (decoder.Frames.Count > 0)
            {                
                var encoders = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
                return new ImageProperties
                {
                    Height = decoder.Frames[0].PixelHeight,
                    Width = decoder.Frames[0].PixelWidth,
                };
            }
            else
            {
                return null;
            }
        }
    }
}
