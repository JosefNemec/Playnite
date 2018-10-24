using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Playnite.Database;
using NLog;
using System.IO;
using Playnite;
using System.Windows.Markup;
using Playnite.Web;
using Playnite.Settings;

namespace PlayniteUI
{
    public class CustomImageStringToImageConverter : MarkupExtension, IValueConverter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
                
        public static GameDatabase Database
        {
            get; set;
        }

        public static object GetImageFromSource(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            var imageId = source;
            if (string.IsNullOrEmpty(imageId))
            {
                return null;
            }

            if (imageId.StartsWith("resources:"))
            {
                return imageId.Replace("resources:", "");
            }

            if (imageId.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var cachedFile = HttpDownloader.GetCachedWebFile(imageId, PlaynitePaths.ImagesCachePath);
                    if (string.IsNullOrEmpty(cachedFile))
                    {
                        logger.Warn("Web file not found: " + imageId);
                        return null;
                    }

                    return BitmapExtensions.BitmapFromFile(cachedFile);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, $"Failed to create bitmap from {imageId} file.");
                    return null;
                }
            }

            if (File.Exists(imageId))
            {
                try
                {
                    return BitmapExtensions.BitmapFromFile(imageId);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to create bitmap from " + imageId);
                    return null;
                }
            }

            try
            {
                if (Database == null)
                {
                    logger.Error("Cannot load database image, database not found.");
                    return null;
                }

                try
                {
                    var imageData = Database.GetFileAsImage(imageId);
                    if (imageData == null)
                    {
                        logger.Warn("Image not found in database: " + imageId);
                        return null;
                    }
                    else
                    {
                        return imageData;
                    }
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, $"Failed to get bitmap from {imageId} database file.");
                    return null;
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to load image from database.");
                return null;
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }

            var image = GetImageFromSource((string)value);
            return image ?? DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
