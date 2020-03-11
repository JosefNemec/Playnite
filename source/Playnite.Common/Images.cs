using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using Playnite.SDK;

namespace Playnite.Common
{
    public class ImageProperties
    {
        public int Height { get; set; }
        public int Width { get; set; }
    }

    public class Images
    {
        private static readonly ILogger logger = LogManager.GetLogger();

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
            try
            {
                using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return GetImageProperties(stream);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to load image properties from file {imagePath}");
                return new ImageProperties();
            }
        }

        public static ImageProperties GetImageProperties(Stream imageStream)
        {
            try
            {
                var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                if (decoder.Frames.HasItems())
                {
                    return new ImageProperties
                    {
                        Height = decoder.Frames.Max(a => a.PixelHeight),
                        Width = decoder.Frames.Max(a => a.PixelWidth),
                    };
                }
                else
                {
                    logger.Warn("Images stream has no frames.");
                    return new ImageProperties();
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to load image properties from stream.");
                return new ImageProperties();
            }
        }
    }
}
